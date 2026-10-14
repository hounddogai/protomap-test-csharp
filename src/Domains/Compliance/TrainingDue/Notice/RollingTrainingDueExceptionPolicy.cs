namespace Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Functions of rolling training due exception policy.</summary>
public static class RollingTrainingDueExceptionPolicy
{
    public static int RollingTrainingDueExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a rolling training due exception code in its canonical RTDE- form.</summary>
    public static string NormalizeRollingTrainingDueExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RTDE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RTDE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
