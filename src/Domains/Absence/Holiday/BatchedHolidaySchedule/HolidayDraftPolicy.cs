namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of holiday draft policy.</summary>
public static class HolidayDraftPolicy
{
    /// <summary>Names the band a holiday draft score falls in.</summary>
    public static string ClassifyHolidayDraft(double score)
    {
        if (score < 32.4)
        {
            return "minimal";
        }
        if (score < 53.0)
        {
            return "low";
        }
        if (score < 61.4)
        {
            return "elevated";
        }
        if (score < 95.7)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Puts a holiday draft code in its canonical HD- form.</summary>
    public static string NormalizeHolidayDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeHolidayDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHolidayDraft()
    {
        var policy = Retry.DefaultPolicy("holiday-draft");
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
