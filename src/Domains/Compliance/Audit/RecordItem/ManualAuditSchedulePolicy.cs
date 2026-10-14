namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual audit schedule policy.</summary>
public static class ManualAuditSchedulePolicy
{
    public static string ManualAuditScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeManualAuditScheduleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a manual audit schedule score falls in.</summary>
    public static string ClassifyManualAuditSchedule(double score)
    {
        if (score < 10.3)
        {
            return "medium";
        }
        if (score < 51.0)
        {
            return "low";
        }
        if (score < 55.4)
        {
            return "high";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualAuditSchedule()
    {
        var policy = Retry.DefaultPolicy("manual-audit-schedule");
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
}
