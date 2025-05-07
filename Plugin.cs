using BepInEx;
using BepInEx.Logging;

namespace Paulov.Tarkov.Matchmaking;

[BepInPlugin("Paulov.Tarkov.LogRedirector", "Paulov.Tarkov.LogRedirector", "2025.5.6")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;

        LoggerRedirectPatches.PatchAllMethods();
    }

}