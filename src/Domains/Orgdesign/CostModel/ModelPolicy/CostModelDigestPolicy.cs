namespace Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of cost model digest policy.</summary>
public static class CostModelDigestPolicy
{
    public static List<string> MergeCostModelDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCostModelDigest()
    {
        var policy = Retry.DefaultPolicy("cost-model-digest");
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

    /// <summary>Puts a cost model digest code in its canonical CMD- form.</summary>
    public static string NormalizeCostModelDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
