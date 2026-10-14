namespace Acme.Workforce.Talent.SkillProfile.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional skill profile manifest.</summary>
public sealed record RegionalSkillProfileManifestView(string CodeText, string Name, string NotesText, string ChecksumText, string Margin, string BudgetText, string WindowDays, string Attempts, string SummaryText, string StatusLabel)
{
    public static RegionalSkillProfileManifestView ToRegionalSkillProfileManifestView(RegionalSkillProfileManifest item)
    {
        return new RegionalSkillProfileManifestView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Notes,
            item.Checksum,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalSkillProfileManifestView> ToRegionalSkillProfileManifestViews(List<RegionalSkillProfileManifest> items)
    {
        var views = new List<RegionalSkillProfileManifestView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalSkillProfileManifestView(item));
        }
        return views;
    }

    public static double SumRegionalSkillProfileManifestView(RegionalSkillProfileManifestView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedMargin + parsedBudget + (double)parsedWindowDays;
    }
}
