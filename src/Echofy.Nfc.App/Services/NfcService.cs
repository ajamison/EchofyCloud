using System.Text;
using Echofy.Nfc.App.Models;
using PCSC;

namespace Echofy.Nfc.App.Services;

public class NfcService
{
    // ── Reader enumeration ─────────────────────────────────────────────────────

    public string[] GetReaders()
    {
        using var ctx = ContextFactory.Instance.Establish(SCardScope.System);
        return ctx.GetReaders();
    }

    // ── Combined wait + read ───────────────────────────────────────────────────

    /// <summary>
    /// Polls until a card is tapped, then reads its NDEF content inside the
    /// same PC/SC connection.  Returns null if the card has no NDEF data.
    /// Throws <see cref="OperationCanceledException"/> when <paramref name="ct"/> fires.
    /// </summary>
    public async Task<string?> WaitAndReadNdefAsync(string readerName, CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            bool connected = false;
            try
            {
                using var ctx    = ContextFactory.Instance.Establish(SCardScope.System);
                using var reader = ctx.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                connected = true;

                var raw  = ReadUserPages(reader);
                if (raw is null) return null;

                var ndef = ExtractNdefMessage(raw);
                if (ndef is null || ndef.Length < 3) return null;

                return DecodeNdefRecord(ndef);
            }
            catch (OperationCanceledException) { throw; }
            catch when (!connected) { /* card not present — poll again */ }
            catch { return null; } // connected but read failed

            await Task.Delay(300, ct);
        }
    }

    // ── Combined wait + write ──────────────────────────────────────────────────

    /// <summary>
    /// Polls until a card is tapped, then writes <paramref name="url"/> as an NDEF
    /// URI record inside the same PC/SC connection.
    /// When <paramref name="password"/> is supplied (4 bytes) the card is authenticated
    /// via PWD_AUTH before any write is attempted.
    /// Throws <see cref="NfcAuthException"/> on wrong password or unsupported reader.
    /// </summary>
    public async Task<bool> WaitAndWriteNdefUrlAsync(
        string readerName, string url, byte[]? password, Action? onCardDetected, CancellationToken ct)
    {
        if (password is { Length: not 4 })
            throw new ArgumentException("Password must be exactly 4 bytes.", nameof(password));

        var pages = PadToPageBoundary(WrapInNdefTlv(BuildUriRecord(url)));

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            bool connected = false;
            try
            {
                using var ctx    = ContextFactory.Instance.Establish(SCardScope.System);
                using var reader = ctx.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                connected = true;
                onCardDetected?.Invoke();

                if (password is not null)
                    AuthenticateNtag(reader, password); // throws NfcAuthException on failure

                for (var i = 0; i < pages.Length; i += 4)
                {
                    var page = (byte)(4 + i / 4);

                    // PC/SC UPDATE BINARY pseudo-APDU: FF D6 00 <page> 04 <b0><b1><b2><b3>
                    var apdu = new byte[]
                    {
                        0xFF, 0xD6, 0x00, page, 0x04,
                        pages[i], pages[i + 1], pages[i + 2], pages[i + 3],
                    };

                    var resp = Transmit(reader, apdu);
                    if (!IsSuccess(resp)) return false;
                }

                return true;
            }
            catch (OperationCanceledException) { throw; }
            catch (NfcAuthException) { throw; }         // surface auth errors immediately
            catch when (!connected) { /* no card — poll again */ }
            catch { return false; }

            await Task.Delay(300, ct);
        }
    }

    // ── Password protection ────────────────────────────────────────────────────

    /// <summary>
    /// Polls until a card is tapped, then configures password protection.
    /// <paramref name="password"/> must be exactly 4 bytes.
    /// When <paramref name="readProtect"/> is false only writes require the password;
    /// when true, reads also require it (phones cannot read the URL without the password).
    /// Returns (Success, CardTypeName). CardTypeName is null when the tag could not be identified.
    /// </summary>
    public async Task<(bool Success, string? CardType)> WaitAndProtectAsync(
        string readerName, byte[] password, bool readProtect, Action? onCardDetected, CancellationToken ct)
    {
        if (password.Length != 4) throw new ArgumentException("Password must be exactly 4 bytes.", nameof(password));

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            bool connected = false;
            try
            {
                using var ctx    = ContextFactory.Instance.Establish(SCardScope.System);
                using var reader = ctx.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                connected = true;
                onCardDetected?.Invoke();

                var info = DetectNtag(reader);
                if (info is null) return (false, null);

                // 1. Write the 4-byte password to the PWD page
                if (!IsSuccess(Transmit(reader,
                    [0xFF, 0xD6, 0x00, info.PwdPage, 0x04,
                     password[0], password[1], password[2], password[3]])))
                    return (false, info.TypeName);

                // 2. Write PACK (2-byte acknowledgement, 0x00 0x00 is fine)
                if (!IsSuccess(Transmit(reader,
                    [0xFF, 0xD6, 0x00, info.PackPage, 0x04, 0x00, 0x00, 0x00, 0x00])))
                    return (false, info.TypeName);

                // 3. Set PROT bit in CFG1 (ACCESS) — preserve other bits
                var cfg1 = Transmit(reader, [0xFF, 0xB0, 0x00, info.CfgAccessPage, 0x04]);
                if (cfg1 is null || cfg1.Length < 6 || !IsSuccess(cfg1)) return (false, info.TypeName);
                byte access = readProtect
                    ? (byte)(cfg1[0] | 0x80)   // PROT=1: read+write protected
                    : (byte)(cfg1[0] & 0x7F);   // PROT=0: write-only protected
                if (!IsSuccess(Transmit(reader,
                    [0xFF, 0xD6, 0x00, info.CfgAccessPage, 0x04, access, cfg1[1], cfg1[2], cfg1[3]])))
                    return (false, info.TypeName);

                // 4. Set AUTH0=0x04 in CFG0 — protect all user pages; preserve mirror bytes
                var cfg0 = Transmit(reader, [0xFF, 0xB0, 0x00, info.AuthPage, 0x04]);
                if (cfg0 is null || cfg0.Length < 6 || !IsSuccess(cfg0)) return (false, info.TypeName);
                if (!IsSuccess(Transmit(reader,
                    [0xFF, 0xD6, 0x00, info.AuthPage, 0x04, cfg0[0], cfg0[1], cfg0[2], 0x04])))
                    return (false, info.TypeName);

                return (true, info.TypeName);
            }
            catch (OperationCanceledException) { throw; }
            catch when (!connected) { /* card not present — poll */ }
            catch { return (false, null); }

            await Task.Delay(300, ct);
        }
    }

    // ── NTAG detection ─────────────────────────────────────────────────────────

    private record NtagInfo(string TypeName, byte AuthPage, byte CfgAccessPage, byte PwdPage, byte PackPage);

    // NTAG type is identified from byte 2 of the Capability Container (page 3).
    // CC[2] encodes user-memory size in 8-byte units. Config pages follow user memory.
    private static NtagInfo? DetectNtag(ICardReader reader)
    {
        var resp = Transmit(reader, [0xFF, 0xB0, 0x00, 3, 0x04]); // read CC (page 3)
        if (resp is null || resp.Length < 6 || !IsSuccess(resp)) return null;

        return resp[2] switch
        {
            0x12 => new NtagInfo("NTAG213", AuthPage: 41,  CfgAccessPage: 42,  PwdPage: 43,  PackPage: 44),
            0x3E => new NtagInfo("NTAG215", AuthPage: 131, CfgAccessPage: 132, PwdPage: 133, PackPage: 134),
            0x6D => new NtagInfo("NTAG216", AuthPage: 227, CfgAccessPage: 228, PwdPage: 229, PackPage: 230),
            _    => null,
        };
    }

    // ── NTAG page reading ──────────────────────────────────────────────────────

    private static byte[]? ReadUserPages(ICardReader reader, byte startPage = 4, int maxPages = 45)
    {
        var data = new List<byte>(maxPages * 4);

        for (var page = startPage; page < startPage + maxPages; page++)
        {
            // PC/SC READ BINARY pseudo-APDU: FF B0 00 <page> 04
            var apdu = new byte[] { 0xFF, 0xB0, 0x00, page, 0x04 };
            var resp = Transmit(reader, apdu);

            if (resp is null || resp.Length < 6 || !IsSuccess(resp)) break;

            data.AddRange(resp[..4]);

            // Stop early once we've seen the NDEF terminator TLV (0xFE)
            if (data.Count > 4 && data.Contains(0xFE)) break;
        }

        return data.Count > 0 ? [.. data] : null;
    }

    // ── NDEF TLV parsing ───────────────────────────────────────────────────────

    private static byte[]? ExtractNdefMessage(byte[] data)
    {
        for (var i = 0; i < data.Length - 2; i++)
        {
            if (data[i] != 0x03) continue; // NDEF Message TLV type

            int len, payloadStart;

            if (data[i + 1] == 0xFF) // three-byte length form
            {
                if (i + 4 > data.Length) return null;
                len          = (data[i + 2] << 8) | data[i + 3];
                payloadStart = i + 4;
            }
            else
            {
                len          = data[i + 1];
                payloadStart = i + 2;
            }

            if (payloadStart + len > data.Length) return null;
            return data[payloadStart..(payloadStart + len)];
        }

        return null;
    }

    // ── NDEF record decoding ───────────────────────────────────────────────────

    private static string? DecodeNdefRecord(byte[] ndef)
    {
        if (ndef.Length < 3) return null;

        var flags       = ndef[0];
        var typeLen     = ndef[1];
        var isShort     = (flags & 0x10) != 0; // SR bit

        if (!isShort) return null; // long records not expected on small tags

        var payloadLen  = ndef[2];
        var typeStart   = 3;
        if (typeStart + typeLen > ndef.Length) return null;

        var type        = Encoding.ASCII.GetString(ndef, typeStart, typeLen);
        var payloadStart = typeStart + typeLen;
        if (payloadStart + payloadLen > ndef.Length) return null;

        var payload = ndef[payloadStart..(payloadStart + payloadLen)];

        return type switch
        {
            "U" => DecodeUri(payload),
            "T" => DecodeText(payload),
            _   => $"[{type}] {BitConverter.ToString(payload)}",
        };
    }

    private static string? DecodeUri(byte[] payload)
    {
        if (payload.Length == 0) return null;

        ReadOnlySpan<string> prefixes =
        [
            "",             // 0x00 – no abbreviation
            "http://www.",  // 0x01
            "https://www.", // 0x02
            "http://",      // 0x03
            "https://",     // 0x04
            "tel:",         // 0x05
            "mailto:",      // 0x06
            "ftp://anonymous:anonymous@", // 0x07
            "ftp://ftp.",   // 0x08
            "ftps://",      // 0x09
            "sftp://",      // 0x0A
            "smb://",       // 0x0B
            "nfs://",       // 0x0C
            "ftp://",       // 0x0D
            "dav://",       // 0x0E
            "news:",        // 0x0F
            "telnet://",    // 0x10
            "imap:",        // 0x11
            "rtsp://",      // 0x12
            "urn:",         // 0x13
            "pop:",         // 0x14
            "sip:",         // 0x15
            "sips:",        // 0x16
            "tftp:",        // 0x17
            "btspp://",     // 0x18
            "btl2cap://",   // 0x19
            "btgoep://",    // 0x1A
            "tcpobex://",   // 0x1B
            "irdaobex://",  // 0x1C
            "file://",      // 0x1D
            "urn:epc:id:",  // 0x1E
            "urn:epc:tag:", // 0x1F
            "urn:epc:pat:", // 0x20
            "urn:epc:raw:", // 0x21
            "urn:epc:",     // 0x22
            "urn:nfc:",     // 0x23
        ];

        var code   = payload[0];
        var prefix = code < prefixes.Length ? prefixes[code] : "";
        return prefix + Encoding.UTF8.GetString(payload[1..]);
    }

    private static string DecodeText(byte[] payload)
    {
        if (payload.Length == 0) return "";
        var langLen = payload[0] & 0x3F;
        return Encoding.UTF8.GetString(payload[(1 + langLen)..]);
    }

    // ── NDEF record building ───────────────────────────────────────────────────

    private static byte[] BuildUriRecord(string url)
    {
        // Choose the longest matching URI prefix abbreviation
        (byte code, string prefix)[] prefixMap =
        [
            (0x02, "https://www."),
            (0x01, "http://www."),
            (0x04, "https://"),
            (0x03, "http://"),
        ];

        byte   prefixCode  = 0x00;
        string strippedUrl = url;

        foreach (var (code, prefix) in prefixMap)
        {
            if (url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                prefixCode  = code;
                strippedUrl = url[prefix.Length..];
                break;
            }
        }

        var urlBytes = Encoding.UTF8.GetBytes(strippedUrl);
        var payload  = (byte[])[prefixCode, .. urlBytes];

        // NDEF record header: MB=1 ME=1 CF=0 SR=1 IL=0 TNF=0x01 = 0xD1
        return
        [
            0xD1,              // flags
            0x01,              // type length (1 byte: 'U')
            (byte)payload.Length, // payload length (SR = single byte)
            (byte)'U',         // type
            .. payload,        // payload: prefix code + stripped URL
        ];
    }

    private static byte[] WrapInNdefTlv(byte[] ndefRecord)
    {
        var len = ndefRecord.Length;

        if (len < 256)
            return [0x03, (byte)len, .. ndefRecord, 0xFE];

        // Three-byte length form for messages >= 256 bytes
        return [0x03, 0xFF, (byte)(len >> 8), (byte)(len & 0xFF), .. ndefRecord, 0xFE];
    }

    private static byte[] PadToPageBoundary(byte[] data)
    {
        var rem = data.Length % 4;
        return rem == 0 ? data : [.. data, .. new byte[4 - rem]];
    }

    // ── NTAG password authentication ───────────────────────────────────────────

    // Sends PWD_AUTH (NFC cmd 0x1B) via the ACR/PN532 DirectTransmit escape APDU.
    // The auth state persists for the lifetime of this ICardReader connection, so
    // all subsequent writes in the same connection succeed without re-authenticating.
    private static void AuthenticateNtag(ICardReader reader, byte[] password)
    {
        // FF 00 00 00 Lc  D4  40  01  1B  P0  P1  P2  P3
        // └─ escape ──┘  └── PN532 InDataExchange tgt=1 ─┘  └── NTAG PWD_AUTH ──┘
        var apdu = new byte[]
        {
            0xFF, 0x00, 0x00, 0x00, 0x07,
            0xD4, 0x40, 0x01,
            0x1B, password[0], password[1], password[2], password[3],
        };

        var resp = Transmit(reader, apdu);

        // Success response: D5 41 00 <PA0> <PA1> 90 00
        //   resp[0]=D5 resp[1]=41 resp[2]=00(status) resp[3-4]=PACK resp[5-6]=SW 90 00
        if (!IsSuccess(resp) || resp!.Length < 5 || resp[2] != 0x00)
            throw new NfcAuthException(
                "Authentication failed — wrong password, or your reader does not support " +
                "NTAG PWD_AUTH via PC/SC (ACR/PN532-based readers are required).");
    }

    // ── APDU helpers ───────────────────────────────────────────────────────────

    private static byte[]? Transmit(ICardReader reader, byte[] apdu)
    {
        try
        {
            var buf = new byte[264];
            var len = reader.Transmit(apdu, buf);
            return buf[..len];
        }
        catch
        {
            return null;
        }
    }

    /// <summary>PC/SC success status words: SW1=0x90, SW2=0x00</summary>
    private static bool IsSuccess(byte[]? resp) =>
        resp is { Length: >= 2 } && resp[^2] == 0x90 && resp[^1] == 0x00;
}
