namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of timesheet template policy.</summary>
public static class TimesheetTemplatePolicy
{
    public static string TimesheetTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a timesheet template code in its canonical TT- form.</summary>
    public static string NormalizeTimesheetTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a timesheet template score falls in.</summary>
    public static string ClassifyTimesheetTemplate(double score)
    {
        if (score < 9.6)
        {
            return "high";
        }
        if (score < 36.5)
        {
            return "critical";
        }
        if (score < 38.7)
        {
            return "ok";
        }
        return "watch";
    }

    public static List<string> MergeTimesheetTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
