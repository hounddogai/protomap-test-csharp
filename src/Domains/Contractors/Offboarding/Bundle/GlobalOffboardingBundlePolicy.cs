namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global offboarding bundle policy.</summary>
public static class GlobalOffboardingBundlePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalOffboardingBundle()
    {
        var policy = Retry.DefaultPolicy("global-offboarding-bundle");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a global offboarding bundle code in its canonical GOB- form.</summary>
    public static string NormalizeGlobalOffboardingBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GOB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GOB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeGlobalOffboardingBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
