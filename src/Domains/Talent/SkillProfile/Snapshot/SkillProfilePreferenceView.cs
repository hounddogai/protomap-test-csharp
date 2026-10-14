namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a skill profile preference.</summary>
public sealed record SkillProfilePreferenceView(string CodeText, string Label, string Country, string MarginText, string ManagerId, string Region, string BatchSize, string Department, string Threshold, string StatusLabel)
{
    public static SkillProfilePreferenceView ToSkillProfilePreferenceView(SkillProfilePreference item)
    {
        return new SkillProfilePreferenceView(
            item.Code,
            item.Label,
            item.Country,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Region,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfilePreferenceView> ToSkillProfilePreferenceViews(List<SkillProfilePreference> items)
    {
        var views = new List<SkillProfilePreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfilePreferenceView(item));
        }
        return views;
    }

    public static double SumSkillProfilePreferenceView(SkillProfilePreferenceView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedBatchSize + parsedThreshold;
    }
}
