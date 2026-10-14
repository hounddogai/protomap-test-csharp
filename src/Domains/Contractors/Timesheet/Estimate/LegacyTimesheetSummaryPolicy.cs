namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy timesheet summary policy.</summary>
public static class LegacyTimesheetSummaryPolicy
{
    public static double LegacyTimesheetSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyTimesheetSummary()
    {
        var policy = Retry.DefaultPolicy("legacy-timesheet-summary");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a legacy timesheet summary code in its canonical LTS- form.</summary>
    public static string NormalizeLegacyTimesheetSummaryCode(string code)
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

    public static string LegacyTimesheetSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
