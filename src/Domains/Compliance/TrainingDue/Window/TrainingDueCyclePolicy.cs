namespace Acme.Workforce.Compliance.TrainingDue.Window;

/// <summary>Functions of training due cycle policy.</summary>
public static class TrainingDueCyclePolicy
{
    /// <summary>Puts a training due cycle code in its canonical TDC- form.</summary>
    public static string NormalizeTrainingDueCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TDC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TDC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a training due cycle score falls in.</summary>
    public static string ClassifyTrainingDueCycle(double score)
    {
        if (score < 8.9)
        {
            return "ok";
        }
        if (score < 44.1)
        {
            return "watch";
        }
        if (score < 71.5)
        {
            return "minimal";
        }
        if (score < 90.5)
        {
            return "medium";
        }
        return "low";
    }

    public static int TrainingDueCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
