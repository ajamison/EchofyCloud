using System.Text.Json;
using Echofy.Nfc.App.Models;
using Echofy.Nfc.App.Services;

// ── Bootstrap ──────────────────────────────────────────────────────────────────
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "Echofy NFC Card Writer";

var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
var config = File.Exists(configPath)
    ? JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(configPath),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AppConfig()
    : new AppConfig();

var nfc = new NfcService();
var api = new EchofyApiService(config.ApiBaseUrl, config.FrontendBaseUrl);

string? token           = null;
string? loginAs         = null;
string? loginRole       = null;
byte[]? nfcPassword     = null;
int?    activeClientId  = config.ClientId;
string? activeClientName = null;
string? reader          = null;

// Auto-select first available reader on startup
try
{
    var found = nfc.GetReaders();
    if (found.Length > 0) reader = found[0];
}
catch { /* No PC/SC service or no reader — handled in menus */ }

// ── Main loop ─────────────────────────────────────────────────────────────────
while (true)
{
    Console.Clear();
    DrawHeader();
    DrawStatus();

    Line("  [1] Login to Echofy");
    if (token is not null)
    {
        Line("  [2] Read card");
        Line("  [3] Write product URL to card");
        Line("  [4] Write custom URL to card");
    }
    if (reader is not null && nfcPassword is not null && (loginRole is "SuperAdmin" or "SuperUser"))
        Line("  [5] Lock card (write-protect)");
    if (token is not null && loginRole is "SuperAdmin" or "SuperUser")
        Line("  [6] Select client");
    Line("  [7] Select NFC reader");
    Line("  [Q] Quit");
    Console.WriteLine();

    Prompt("  > ");
    var key = Console.ReadLine()?.Trim().ToUpper();

    switch (key)
    {
        case "1": await DoLogin(); break;
        case "2" when token is not null: await DoReadCard(); break;
        case "3" when token is not null: await DoWriteProductUrl(); break;
        case "4" when token is not null: await DoWriteCustomUrl(); break;
        case "5" when reader is not null && nfcPassword is not null && loginRole is "SuperAdmin" or "SuperUser": await DoLockCard(); break;
        case "6" when token is not null && loginRole is "SuperAdmin" or "SuperUser": await DoSelectClient(); break;
        case "7": DoSelectReader(); break;
        case "Q": return;
    }
}

// ── Handlers ──────────────────────────────────────────────────────────────────

async Task DoLogin()
{
    Console.Clear();
    DrawHeader();
    Heading("  Login to Echofy");
    Console.WriteLine();
    Info($"  API: {config.ApiBaseUrl}");
    Console.WriteLine();

    Prompt("  Email: ");
    var email = Console.ReadLine()?.Trim() ?? "";

    Prompt("  Password: ");
    var password = ReadMasked();
    Console.WriteLine();
    Console.WriteLine();

    // Clear previous session
    token = null; loginAs = null; loginRole = null; nfcPassword = null;
    activeClientId = config.ClientId; activeClientName = null;

    Muted("  Authenticating...");

    try
    {
        var resp = await api.LoginAsync(email, password);
        if (resp is null)
        {
            Fail("  Login failed — check your credentials.");
        }
        else
        {
            token     = resp.Token;
            loginAs   = resp.Email;
            loginRole = resp.Role;
            Pass($"  Logged in as {resp.FullName} ({resp.Role})");

            if (resp.Role is "SuperAdmin" or "SuperUser")
            {
                Console.WriteLine();
                if (activeClientId.HasValue)
                    await LoadNfcSettings();
                else
                    Warn("  Tip: use [6] Select client to load the NFC lock password.");
            }
        }
    }
    catch (Exception ex)
    {
        Fail($"  Error: {ex.Message}");
    }

    Pause();
}

async Task DoReadCard()
{
    Console.Clear();
    DrawHeader();
    Heading("  Read NFC Card");
    Console.WriteLine();

    if (!CheckReader()) return;

    Warn("  Tap a card to the reader...");
    Muted("  (press any key to cancel)");
    Console.WriteLine();

    using var cts = new CancellationTokenSource();

    string? content = null;
    Exception? err  = null;

    var workerTask = Task.Run(async () =>
    {
        try
        {
            content = await nfc.WaitAndReadNdefAsync(reader!, cts.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { err = ex; }
        finally { if (!cts.IsCancellationRequested) await cts.CancelAsync(); }
    });

    bool userCancelled = await WaitOrCancel(cts);
    await workerTask;

    Console.WriteLine();

    if (err is not null)
        Fail($"  Error: {err.Message}");
    else if (userCancelled && content is null)
        Muted("  Cancelled.");
    else if (content is null)
    {
        Fail("  No NDEF data found on this card.");
        Muted("  The card may be blank, use a proprietary format, or be incompatible.");
    }
    else
    {
        Pass("  Card read successfully!");
        Console.WriteLine();
        Console.Write("  Content : ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(content);
        Console.ResetColor();
    }

    Pause();
}

async Task DoWriteProductUrl()
{
    Console.Clear();
    DrawHeader();
    Heading("  Write Product URL to Card");
    Console.WriteLine();

    if (!CheckReader()) return;

    Muted("  Fetching assigned short IDs from Echofy...");
    Console.WriteLine();

    List<ProductShortIdDto> shortIds;
    try
    {
        shortIds = await api.GetAssignedShortIdsAsync(token!);
    }
    catch (Exception ex)
    {
        Fail($"  Failed to fetch short IDs: {ex.Message}");
        Pause();
        return;
    }

    if (shortIds.Count == 0)
    {
        Warn("  No assigned product short IDs found.");
        Muted("  Go to Admin › QR Labels in Echofy and assign a product to a label first.");
        Pause();
        return;
    }

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"  {"#",-4} {"Product",-38} {"Code",-12} URL");
    Console.WriteLine($"  {"─",-4} {"─",-38} {"─",-12} ───");
    Console.ResetColor();

    for (var i = 0; i < shortIds.Count; i++)
    {
        var s   = shortIds[i];
        var url = api.BuildProductUrl(s.Code);
        var lbl = Truncate(s.ProductName ?? s.Label ?? "(unlabelled)", 38);
        Console.Write($"  {i + 1,-4} {lbl,-38} ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{s.Code,-12}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($" {url}");
        Console.ResetColor();
    }

    Console.WriteLine();
    Prompt("  Select number: ");
    if (!int.TryParse(Console.ReadLine()?.Trim(), out var choice) || choice < 1 || choice > shortIds.Count)
    {
        Warn("  Invalid selection.");
        Pause();
        return;
    }

    var selected = shortIds[choice - 1];
    var writeUrl = api.BuildProductUrl(selected.Code);

    await WriteUrlToCard(writeUrl, nfcPassword);
}

async Task DoWriteCustomUrl()
{
    Console.Clear();
    DrawHeader();
    Heading("  Write Custom URL to Card");
    Console.WriteLine();

    if (!CheckReader()) return;

    Prompt("  URL: ");
    var url = Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
    {
        Warn("  Invalid URL. Must be an absolute URL (http:// or https://).");
        Pause();
        return;
    }

    await WriteUrlToCard(url, nfcPassword);
}

async Task WriteUrlToCard(string url, byte[]? password = null)
{
    Console.WriteLine();
    Console.Write("  Writing : ");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(url);
    Console.ResetColor();
    if (password is not null)
    {
        Console.Write("  Auth    : ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("NFC lock password will be used");
        Console.ResetColor();
    }
    Console.WriteLine();
    Warn("  Tap a card to the reader...");
    Muted("  (press any key to cancel)");
    Console.WriteLine();

    using var cts = new CancellationTokenSource();

    bool success  = false;
    Exception? err = null;

    var workerTask = Task.Run(async () =>
    {
        try
        {
            success = await nfc.WaitAndWriteNdefUrlAsync(reader!, url, password,
                () => Muted("  Card detected — writing..."), cts.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { err = ex; }
        finally { if (!cts.IsCancellationRequested) await cts.CancelAsync(); }
    });

    bool userCancelled = await WaitOrCancel(cts);
    await workerTask;

    Console.WriteLine();

    if (err is not null)
        Fail($"  Error: {err.Message}");
    else if (userCancelled && !success)
        Muted("  Cancelled.");
    else if (!success)
    {
        Fail("  Write failed — the card may be locked or incompatible.");
        Muted("  NTAG213/215/216 and MIFARE Ultralight cards are supported.");
    }
    else
    {
        Pass("  Card written successfully!");
        Console.WriteLine();
        Muted($"  Tap the card to a phone to open: {url}");
    }

    Pause();
}

async Task DoLockCard()
{
    Console.Clear();
    DrawHeader();
    Heading("  Lock Card (Write-Protect)");
    Console.WriteLine();

    if (!CheckReader()) return;

    // nfcPassword is guaranteed non-null here (menu is gated), but be defensive
    if (nfcPassword is null)
    {
        Fail("  NFC lock password not configured.");
        Muted("  A SuperAdmin must set it in Admin › NFC Settings in the Echofy web app.");
        Pause();
        return;
    }

    Info("  Locks the card so only the configured password can overwrite it.");
    Info("  Phones can still tap and read the URL freely (write-only protection).");
    Console.WriteLine();
    Console.Write("  Password : ");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(BitConverter.ToString(nfcPassword));
    Console.ResetColor();
    Console.WriteLine();

    Prompt("  Confirm lock? [Y/N]: ");
    if (!string.Equals(Console.ReadLine()?.Trim(), "Y", StringComparison.OrdinalIgnoreCase))
    {
        Muted("  Aborted.");
        Pause();
        return;
    }

    Console.WriteLine();
    Warn("  Tap a card to the reader...");
    Muted("  (press any key to cancel)");
    Console.WriteLine();

    using var cts = new CancellationTokenSource();

    bool success = false;
    string? cardType = null;
    Exception? err = null;

    var workerTask = Task.Run(async () =>
    {
        try
        {
            (success, cardType) = await nfc.WaitAndProtectAsync(reader!, nfcPassword, readProtect: false,
                () => Muted("  Card detected — locking..."), cts.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { err = ex; }
        finally { if (!cts.IsCancellationRequested) await cts.CancelAsync(); }
    });

    bool userCancelled = await WaitOrCancel(cts);
    await workerTask;

    Console.WriteLine();

    if (err is not null)
        Fail($"  Error: {err.Message}");
    else if (userCancelled && !success)
        Muted("  Cancelled.");
    else if (cardType is null)
    {
        Fail("  Could not identify card type.");
        Muted("  Only NTAG213, NTAG215, and NTAG216 cards are supported.");
    }
    else if (!success)
        Fail($"  Failed to lock {cardType} — the card may already be protected.");
    else
    {
        Pass($"  {cardType} locked successfully!");
        Console.WriteLine();
        Muted("  Mode: Write-only protected — phones can still read the URL.");
    }

    Pause();
}

async Task LoadNfcSettings()
{
    if (token is null || !activeClientId.HasValue) return;
    try
    {
        Muted("  Fetching NFC settings...");
        var settings = await api.GetNfcSettingsAsync(activeClientId.Value, token);
        if (settings is not null)
        {
            activeClientName = settings.ClientName;
            nfcPassword = settings.Password is not null ? Convert.FromHexString(settings.Password) : null;
            if (nfcPassword is not null)
                Pass($"  NFC lock password loaded for {settings.ClientName}.");
            else
                Warn($"  No NFC lock password set for {settings.ClientName} — configure it in Admin › NFC Settings.");
        }
        else
        {
            Warn("  Could not fetch NFC settings — check the ClientId.");
        }
    }
    catch (Exception ex) { Warn($"  Could not fetch NFC settings: {ex.Message}"); }
}

async Task DoSelectClient()
{
    Console.Clear();
    DrawHeader();
    Heading("  Select Client");
    Console.WriteLine();

    Muted("  Fetching clients...");

    List<ClientSummaryDto> clients;
    try
    {
        clients = await api.GetClientsAsync(token!);
    }
    catch (Exception ex)
    {
        Fail($"  Failed to fetch clients: {ex.Message}");
        Pause();
        return;
    }

    if (clients.Count == 0)
    {
        Warn("  No clients found.");
        Pause();
        return;
    }

    Console.Clear();
    DrawHeader();
    Heading("  Select Client");
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"  {"#",-4} {"Client",-40} Active");
    Console.WriteLine($"  {"─",-4} {"─",-40} ──────");
    Console.ResetColor();

    for (var i = 0; i < clients.Count; i++)
    {
        var c = clients[i];
        Console.Write($"  {i + 1,-4} {Truncate(c.Name, 40),-40} ");
        if (c.Id == activeClientId)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("← current");
            Console.ResetColor();
        }
        else if (!c.IsActive)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("inactive");
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    Console.WriteLine();
    Prompt("  Select number: ");
    if (!int.TryParse(Console.ReadLine()?.Trim(), out var choice) || choice < 1 || choice > clients.Count)
    {
        Warn("  Invalid selection — client unchanged.");
        Pause();
        return;
    }

    var selected = clients[choice - 1];
    activeClientId   = selected.Id;
    activeClientName = selected.Name;
    nfcPassword      = null;

    Console.WriteLine();
    Pass($"  Client set to: {selected.Name}");
    Console.WriteLine();

    await LoadNfcSettings();
    Pause();
}

void DoSelectReader()
{
    Console.Clear();
    DrawHeader();
    Heading("  Select NFC Reader");
    Console.WriteLine();

    string[] readers;
    try
    {
        readers = nfc.GetReaders();
    }
    catch (Exception ex)
    {
        Fail($"  PC/SC service error: {ex.Message}");
        Muted("  Ensure the Smart Card service is running (services.msc → SCardSvr).");
        Pause();
        return;
    }

    if (readers.Length == 0)
    {
        Warn("  No PC/SC readers detected.");
        Muted("  Connect your NFC reader and try again.");
        Pause();
        return;
    }

    for (var i = 0; i < readers.Length; i++)
    {
        Console.Write($"  [{i + 1}] {readers[i]}");
        if (readers[i] == reader)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" ← current");
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    Console.WriteLine();
    Prompt("  Choice: ");
    if (int.TryParse(Console.ReadLine()?.Trim(), out var idx) && idx >= 1 && idx <= readers.Length)
    {
        reader = readers[idx - 1];
        Pass($"  Reader set to: {reader}");
    }
    else
    {
        Warn("  Invalid selection — reader unchanged.");
    }

    Pause();
}

// ── Shared helpers ─────────────────────────────────────────────────────────────

bool CheckReader()
{
    if (reader is not null) return true;
    Fail("  No NFC reader selected.");
    Muted("  Press [7] from the main menu to select a reader.");
    Pause();
    return false;
}

/// <summary>Runs until a keypress cancels via <paramref name="cts"/>.</summary>
/// <summary>Returns true if the user pressed a key to cancel, false if the worker finished first.</summary>
static async Task<bool> WaitOrCancel(CancellationTokenSource cts)
{
    while (!cts.Token.IsCancellationRequested)
    {
        if (Console.KeyAvailable)
        {
            Console.ReadKey(true);
            await cts.CancelAsync();
            return true;
        }
        await Task.Delay(50);
    }
    return false;
}

static string ReadMasked()
{
    var sb = new System.Text.StringBuilder();
    while (true)
    {
        var k = Console.ReadKey(intercept: true);
        if (k.Key == ConsoleKey.Enter) break;
        if (k.Key == ConsoleKey.Backspace)
        {
            if (sb.Length > 0) { sb.Remove(sb.Length - 1, 1); Console.Write("\b \b"); }
        }
        else
        {
            sb.Append(k.KeyChar);
            Console.Write('*');
        }
    }
    return sb.ToString();
}

static string Truncate(string s, int max) =>
    s.Length <= max ? s : s[..(max - 1)] + "…";

// ── Console drawing ────────────────────────────────────────────────────────────

void DrawHeader()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine();
    Console.WriteLine("  ╔════════════════════════════════════════╗");
    Console.WriteLine("  ║       Echofy NFC Card Writer           ║");
    Console.WriteLine("  ╚════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
}

void DrawStatus()
{
    StatusRow("Reader",   reader ?? "None selected",                          reader is not null);
    StatusRow("API",      loginAs ?? "Not logged in",                         token  is not null);
    StatusRow("Client",   activeClientName ?? (activeClientId.HasValue ? $"ID {activeClientId}" : "None"), activeClientId.HasValue);
    StatusRow("NFC Lock", nfcPassword is not null ? "Configured" : "Not set", nfcPassword is not null);
    StatusRow("Server",   config.ApiBaseUrl,                                   true);

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  ──────────────────────────────────────────");
    Console.ResetColor();
    Console.WriteLine();
}

static void StatusRow(string label, string value, bool ok)
{
    Console.Write($"  {label,-8}: ");
    Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.DarkYellow;
    Console.WriteLine(value.Length > 55 ? value[..52] + "…" : value);
    Console.ResetColor();
}

static void Heading(string s)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(s);
    Console.ResetColor();
}

static void Line(string s)   => Console.WriteLine(s);
static void Info(string s)   { Console.ForegroundColor = ConsoleColor.DarkCyan;   Console.WriteLine(s); Console.ResetColor(); }
static void Pass(string s)   { Console.ForegroundColor = ConsoleColor.Green;      Console.WriteLine(s); Console.ResetColor(); }
static void Fail(string s)   { Console.ForegroundColor = ConsoleColor.Red;        Console.WriteLine(s); Console.ResetColor(); }
static void Warn(string s)   { Console.ForegroundColor = ConsoleColor.Yellow;     Console.WriteLine(s); Console.ResetColor(); }
static void Muted(string s)  { Console.ForegroundColor = ConsoleColor.DarkGray;   Console.WriteLine(s); Console.ResetColor(); }
static void Prompt(string s) { Console.ForegroundColor = ConsoleColor.White;      Console.Write(s);     Console.ResetColor(); }
static void Pause()          { Console.WriteLine(); Muted("  Press any key to continue..."); Console.ReadKey(true); }
