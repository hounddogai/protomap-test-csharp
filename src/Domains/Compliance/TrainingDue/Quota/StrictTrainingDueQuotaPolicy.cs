namespace Acme.Workforce.Compliance.TrainingDue.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict training due quota policy.</summary>
public static class StrictTrainingDueQuotaPolicy
{
    public static List<string> MergeStrictTrainingDueQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictTrainingDueQuota()
    {
        var policy = Retry.DefaultPolicy("strict-training-due-quota");
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

    /// <summary>Puts a strict training due quota code in its canonical STDQ- form.</summary>
    public static string NormalizeStrictTrainingDueQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("STDQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"STDQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a strict training due quota score falls in.</summary>
    public static string ClassifyStrictTrainingDueQuota(double score)
    {
        if (score < 59.8)
        {
            return "critical";
        }
        if (score < 83.9)
        {
            return "ok";
        }
        if (score < 96.5)
        {
            return "low";
        }
        if (score < 97.3)
        {
            return "elevated";
        }
        return "medium";
    }
}
