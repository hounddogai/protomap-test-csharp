namespace Acme.Workforce.Compliance.TrainingDue.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of training due window policy.</summary>
public static class TrainingDueWindowPolicy
{
    /// <summary>Names the band a training due window score falls in.</summary>
    public static string ClassifyTrainingDueWindow(double score)
    {
        if (score < 38.7)
        {
            return "critical";
        }
        if (score < 74.3)
        {
            return "watch";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTrainingDueWindow()
    {
        var policy = Retry.DefaultPolicy("training-due-window");
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

    public static List<string> MergeTrainingDueWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a training due window code in its canonical TDW- form.</summary>
    public static string NormalizeTrainingDueWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TDW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TDW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
