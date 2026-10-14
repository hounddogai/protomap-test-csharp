namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient timesheet score policy.</summary>
public static class LenientTimesheetScorePolicy
{
    public static string LenientTimesheetScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a lenient timesheet score code in its canonical LTS- form.</summary>
    public static string NormalizeLenientTimesheetScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LTS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LTS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
