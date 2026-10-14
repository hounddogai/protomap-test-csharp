namespace Acme.Workforce.Talent.SkillProfile.Summary;

using System.Globalization;

/// <summary>How screens and exports show a adaptive skill profile summary.</summary>
public sealed record AdaptiveSkillProfileSummaryView(string CodeText, string Name, string ArchivedText, string WindowDays, string Summary, string StatusLabel)
{
    public static AdaptiveSkillProfileSummaryView ToAdaptiveSkillProfileSummaryView(AdaptiveSkillProfileSummary item)
    {
        return new AdaptiveSkillProfileSummaryView(
            item.Code,
            item.Name,
            $"{item.Archived}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveSkillProfileSummaryView> ToAdaptiveSkillProfileSummaryViews(List<AdaptiveSkillProfileSummary> items)
    {
        var views = new List<AdaptiveSkillProfileSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveSkillProfileSummaryView(item));
        }
        return views;
    }

    public static double SumAdaptiveSkillProfileSummaryView(AdaptiveSkillProfileSummaryView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays;
    }
}
