using EFT;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace Paulov.Tarkov.LogRedirector
{
    internal class LoggerRedirectPatches
    {
        public static void PatchAllMethods()
        {
            // Get the type of Abstract Logger that is used by all consuming Loggers
            Type[] loggerTypes = typeof(TarkovApplication).Assembly.GetTypes().Where(t => t.IsAbstract && t.GetMethods().Any(x => x.Name == "LogDebug")).ToArray();

            foreach (var loggerType in loggerTypes)
            {
                Plugin.Logger.LogInfo($"Found logger type: {loggerType.Name}");

                var logDebugMethods = loggerType
                    .GetMethods(
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public)
                    .Where(x => (x.Name.StartsWith("LogDebug") || (x.Name.StartsWith("LogInfo"))));

                // Iterate through each method and patch it
                foreach (var method in logDebugMethods)
                {
                    // Create Harmony instance for the method
                    var harmony = new Harmony($"{loggerType}.{method.Name}");

                    // Create a Harmony patch for the method
                    var harmonyMethod = new HarmonyMethod(typeof(LoggerRedirectPatches).GetMethod("Postfix"), debug: false);
                    try
                    {
                        // Patch the method with the prefix
                        harmony.Patch(method, null, harmonyMethod, null, null, null);

                        Plugin.Logger.LogInfo($"Patching {method.Name}");
                    }
                    catch
                    {
                        harmony.Unpatch(method, HarmonyPatchType.All);

                        Plugin.Logger.LogError($"Failed to patch {method.Name}");
                    }
                }
            }
        }

        public static void Postfix(MethodBase __originalMethod, string format, object[] args)
        {
            //Plugin.Logger.LogDebug($"{__originalMethod.Name}");

            var parameters = __originalMethod.GetParameters();

            switch (__originalMethod.Name)
            {
                case "LogInfo":
                case "LogDebug":
                    if (parameters.Length > 0)
                    {
                        if (format.Contains("{0}"))
                        {
                            var formatting = format.ToString();
                            //Plugin.Logger.LogDebug(formatting);


                            for (var i = 0; i < args.Length; i++)
                            {
                                var argValue = args[i];
                                if (argValue == null)
                                    argValue = "null";

                                formatting = formatting.Replace("{" + i + "}", argValue.ToString());
                            }

                            if (__originalMethod.Name == "LogInfo")
                            {
                                Plugin.Logger.LogInfo(formatting);
                            }
                            else
                            {
                                Plugin.Logger.LogDebug(formatting);
                            }
                        }
                        else
                        {
                        }
                    }

                    break;
                case "LogError":
                    break;
                case "LogWarning":
                    break;
                default:
                    break;
            }

            //var parameters = __originalMethod.GetParameters();
            //for (var i = 0; i < parameters.Length; i++)
            //    Plugin.Logger.LogDebug($"{parameters[i].Name} of type {parameters[i].ParameterType}");
        }

    }
}
