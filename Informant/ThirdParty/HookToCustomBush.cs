using Informant.ThirdParty.CustomBush;
using Slothsoft.Informant;
using System.Diagnostics.CodeAnalysis;

namespace Informant.ThirdParty;

public static class HookToCustomBush
{
    private const string ModId = "furyx639.CustomBush";
    private static readonly Dictionary<string, object> RegisteredApis = new();

    public static void Apply(InformantMod informantMod)
    {
        var customBush = informantMod.Helper.ModRegistry.GetApi<ICustomBushApi>(ModId);
        if (customBush == null) {
            return;
        }
        RegisteredApis[ModId] = customBush;
    }

    public static bool GetApi<T>([NotNullWhen(true)] out T? apiInstance) where T : class
    {
        apiInstance = null;
        if (!RegisteredApis.TryGetValue(ModId, out object? api)) {
            return false;
        }

        if (api is T apiVal) {
            apiInstance = apiVal;
            return true;
        }
        return false;
    }
}
