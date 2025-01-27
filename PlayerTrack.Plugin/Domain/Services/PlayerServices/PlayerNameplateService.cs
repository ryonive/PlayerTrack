﻿using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using PlayerTrack.Data;
using PlayerTrack.Models;

namespace PlayerTrack.Domain;

public class PlayerNameplateService
{
    public static PlayerNameplate GetPlayerNameplate(Player player, LocationType locationType)
    {
        Plugin.PluginLog.Verbose($"Entering PlayerNameplateService.GetPlayerNameplate(): {player.Id}, {locationType}");
        var nameplate = new PlayerNameplate
        {
            CustomizeNameplate = locationType switch
            {
                LocationType.Overworld => PlayerConfigService.GetNameplateShowInOverworld(player),
                LocationType.Content => PlayerConfigService.GetNameplateShowInContent(player),
                LocationType.HighEndContent => PlayerConfigService.GetNameplateShowInHighEndContent(player),
                _ => false,
            },
        };

        if (!nameplate.CustomizeNameplate)
        {
            Plugin.PluginLog.Debug($"CustomizeNameplate is false for {player.Name}");
            return nameplate;
        }

        var isColorEnabled = PlayerConfigService.GetNameplateUseColor(player);
        ushort color = 0;
        if (isColorEnabled)
        {
            color = (ushort)PlayerConfigService.GetNameplateColor(player);
            nameplate.TitleLeftQuote = new SeString().Append(new UIForegroundPayload(color)).Append("《");
            nameplate.TitleRightQuote = new SeString().Append("》").Append(UIForegroundPayload.UIForegroundOff);
            nameplate.NameTextWrap = (new SeString(new UIForegroundPayload(color)), new SeString(UIForegroundPayload.UIForegroundOff));
            nameplate.FreeCompanyLeftQuote = new SeString().Append(new UIForegroundPayload(color)).Append(" «");
            nameplate.FreeCompanyRightQuote = new SeString().Append("»").Append(UIForegroundPayload.UIForegroundOff);
        }

        nameplate.NameplateUseColorIfDead = PlayerConfigService.GetNameplateUseColorIfDead(player);

        var nameplateTitleType = PlayerConfigService.GetNameplateTitleType(player);

        var title = nameplateTitleType switch
        {
            NameplateTitleType.CustomTitle => PlayerConfigService.GetNameplateCustomTitle(player),
            NameplateTitleType.CategoryName when player.PrimaryCategoryId != 0 => ServiceContext.CategoryService
                .GetCategory(player.PrimaryCategoryId)
                ?.Name ?? string.Empty,
            _ => string.Empty
        };

        if (nameplateTitleType != NameplateTitleType.NoChange && !string.IsNullOrEmpty(title))
        {
            nameplate.CustomTitle = title;
            nameplate.HasCustomTitle = true;
        }

        if ((!isColorEnabled || color == 0) && !nameplate.HasCustomTitle)
            nameplate.CustomizeNameplate = false;

        return nameplate;
    }
}
