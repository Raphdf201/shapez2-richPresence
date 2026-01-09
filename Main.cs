using Core.Localization;
using Discord;
using ShapezShifter.Flow;

namespace shapez2RichPresence;

public class Main : IMod
{
    private readonly Discord.Discord _discord = new(1458910861564313776, (ulong)CreateFlags.NoRequireDiscord);
    private int _ticksSinceLastUpdate;
    private float _totalPlayTime;
    private string _scenario = "";
    private int _buildings;

    public Main()
    {
        if (!File.Exists(SdkDownloader.GetTargetSdkLocation())) SdkDownloader.DownloadSdk();
        this.RunPeriodically(Periodic);
        this.RegisterConsoleCommand("stats", context =>
        {
            context.Output($"Ticks since last update {_ticksSinceLastUpdate}");
            context.Output($"Scenario {_scenario}");
            context.Output($"Buildings {_buildings}");
        });
        this.RegisterConsoleCommand("update", _ => UpdateActivity());
    }

    public void Dispose()
    {
        _discord.Dispose();
    }

    private void UpdateActivity()
    {
        _discord.GetActivityManager().UpdateActivity(new Activity
        {
            State = "In game",
            Details = _scenario + " - " + _buildings + " buildings",
            Assets =
            {
                LargeImage = "large-image",
                LargeText = "Idk what to put here",
                SmallImage = "https://cdn.impress.games/presskits/b05b8fc32a22c183a871c2d84a102828/logos/3.png",
                SmallText = _totalPlayTime / 60 / 60 + " hours"
            }
        }, _ => { });
    }

    private void Periodic(GameSessionOrchestrator orchestrator, float deltaTick)
    {
        _scenario = orchestrator.DependencyContainer.TryResolve<ILocalizationResolver>(out var resolver)
            ? orchestrator.Mode.Scenario.Title.Build(resolver, null)
            : "No scenario";
        _buildings = orchestrator.LocalPlayer.CurrentMap.BuildingCount;
        _totalPlayTime = orchestrator.LocalPlayer.TotalPlaytime;

        _ticksSinceLastUpdate++;
        if (_ticksSinceLastUpdate > 500)
        {
            UpdateActivity();
            _ticksSinceLastUpdate = 0;
        }

        _discord.RunCallbacks();
    }
}
