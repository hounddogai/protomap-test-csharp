namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient training due schedule policy.</summary>
public static class LenientTrainingDueSchedulePolicy
{
    public static int LenientTrainingDueScheduleSpan(int start, int end)
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
        return span / 3600;
    }

    public static string LenientTrainingDueScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a lenient training due schedule code in its canonical LTDS- form.</summary>
    public static string NormalizeLenientTrainingDueScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LTDS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LTDS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
