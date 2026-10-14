namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pay band rule policy.</summary>
public static class PayBandRulePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayBandRule()
    {
        var policy = Retry.DefaultPolicy("pay-band-rule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a pay band rule code in its canonical PBR- form.</summary>
    public static string NormalizePayBandRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePayBandRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
