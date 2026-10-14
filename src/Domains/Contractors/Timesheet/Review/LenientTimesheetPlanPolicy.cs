namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient timesheet plan policy.</summary>
public static class LenientTimesheetPlanPolicy
{
    public static string LenientTimesheetPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a lenient timesheet plan code in its canonical LTP- form.</summary>
    public static string NormalizeLenientTimesheetPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LTP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LTP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LenientTimesheetPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
