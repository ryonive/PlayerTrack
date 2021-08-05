using System;
using System.Numerics;

using Dalamud.DrunkenToad;
using XivCommon.Functions.NamePlates;

namespace PlayerTrack
{
    /// <summary>
    /// Manage name plates for players.
    /// </summary>
    public class NamePlateManager
    {
        private readonly PlayerTrackPlugin plugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamePlateManager"/> class.
        /// </summary>
        /// <param name="plugin">plugin.</param>
        public NamePlateManager(PlayerTrackPlugin plugin)
        {
            this.plugin = plugin;
            this.plugin.XivCommon.Functions.NamePlates.OnUpdate += this.OnNamePlateUpdate;
        }

        /// <summary>
        /// Dispose name plates manager.
        /// </summary>
        public void Dispose()
        {
            this.plugin.XivCommon.Functions.NamePlates.OnUpdate -= this.OnNamePlateUpdate;
        }

        /// <summary>
        /// Force existing nameplates to redraw.
        /// </summary>
        public void ForceRedraw()
        {
            this.plugin.XivCommon.Functions.NamePlates.ForceRedraw = true;
        }

        private void OnNamePlateUpdate(NamePlateUpdateEventArgs args)
        {
            try
            {
                // check if any nameplate features enabled
                if (!this.plugin.Configuration.ChangeNamePlateTitleToCategory &&
                    !this.plugin.Configuration.UseNamePlateColors) return;

                // check if plugin started
                if (!this.plugin.IsDoneLoading) return;

                // check if valid
                if (args.Type != PlateType.Player || args.ActorId == 0) return;

                // get player
                var actor = this.plugin.ActorManager.GetPlayerCharacter(args.ActorId);
                if (actor == null) return;
                var player = this.plugin.PlayerService.GetPlayer(actor.Name, (ushort)actor.HomeWorld.Id);
                if (player == null) return;

                // set title
                if (this.plugin.Configuration.ChangeNamePlateTitleToCategory)
                {
                    var category = this.plugin.CategoryService.GetCategory(player.CategoryId);
                    if (category.IsDefault == false && category.Title != null)
                    {
                        args.Title = category.Title;
                        args.Type = PlateType.LowTitleNoFc;
                    }
                }

                // set color
                if (this.plugin.Configuration.UseNamePlateColors)
                {
                    var color = this.plugin.PlayerService.GetPlayerNamePlateColor(player);
                    if (color != null)
                    {
                        args.Colour = ((Vector4)color).ToByteColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to update nameplate.");
            }
        }
    }
}