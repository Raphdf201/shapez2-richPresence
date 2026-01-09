using Core.Localization;
using Discord;
using ShapezShifter.Flow;

namespace shapez2RichPresence;

public class Main : IMod
{
    private static readonly Discord.Discord Discord = new(1458910861564313776, (ulong)CreateFlags.NoRequireDiscord);
    private static int _ticksSinceLastUpdate;
    private static float _totalPlayTime;
    private static string _scenario = "";
    private static int _buildings;

    public Main()
    {
        this.RunPeriodically(Periodic);
        this.RegisterConsoleCommand("stats", context =>
        {
            context.Output($"Ticks since last update {_ticksSinceLastUpdate}");
            context.Output($"Scenario {_scenario}");
            context.Output($"Buildings {_buildings}");
        });
        this.RegisterConsoleCommand("update", _ => UpdateActivity());
    }

    private static void UpdateActivity()
    {
        Discord.GetActivityManager().UpdateActivity(new Activity
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
        }, _ => {});
    }

    private static void Periodic(GameSessionOrchestrator orchestrator, float deltaTick)
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
        
        Discord.RunCallbacks();
    }

    public void Dispose()
    {
        Discord.Dispose();
    }
}
