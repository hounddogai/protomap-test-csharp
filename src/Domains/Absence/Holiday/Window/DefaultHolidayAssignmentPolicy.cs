namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default holiday assignment policy.</summary>
public static class DefaultHolidayAssignmentPolicy
{
    public static int DefaultHolidayAssignmentSpan(int start, int end)
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
        return span / 86400;
    }

    public static List<string> MergeDefaultHolidayAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a default holiday assignment score falls in.</summary>
    public static string ClassifyDefaultHolidayAssignment(double score)
    {
        if (score < 50.5)
        {
            return "low";
        }
        if (score < 60.7)
        {
            return "high";
        }
        if (score < 70.2)
        {
            return "elevated";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultHolidayAssignment()
    {
        var policy = Retry.DefaultPolicy("default-holiday-assignment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
