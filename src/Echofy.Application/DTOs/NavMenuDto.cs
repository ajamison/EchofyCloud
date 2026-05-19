namespace Echofy.Application.DTOs;

public record NavMenuItemDto(
    string Text,
    string Controller,
    string Action,
    string ActivePage);

public record NavMenuGroupDto(
    string Label,
    string Icon,
    string CollapseId,
    IReadOnlyList<NavMenuItemDto> Items);
