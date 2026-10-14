namespace Acme.Workforce.Contractors.Timesheet.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of timesheet record policy.</summary>
public static class TimesheetRecordPolicy
{
    public static List<string> MergeTimesheetRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double TimesheetRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a timesheet record score falls in.</summary>
    public static string ClassifyTimesheetRecord(double score)
    {
        if (score < 27.6)
        {
            return "watch";
        }
        if (score < 54.5)
        {
            return "elevated";
        }
        if (score < 67.6)
        {
            return "high";
        }
        return "low";
    }

    public static string TimesheetRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
