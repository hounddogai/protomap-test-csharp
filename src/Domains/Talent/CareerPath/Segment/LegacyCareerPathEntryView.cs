namespace Acme.Workforce.Talent.CareerPath.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy career path entry.</summary>
public sealed record LegacyCareerPathEntryView(string Code, string Name, string FteText, string Skill, string Segment, string Quantity, string TimezoneText, string StatusLabel)
{
    public static LegacyCareerPathEntryView ToLegacyCareerPathEntryView(LegacyCareerPathEntry item)
    {
        return new LegacyCareerPathEntryView(
            item.Code,
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.Segment, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyCareerPathEntryView> ToLegacyCareerPathEntryViews(List<LegacyCareerPathEntry> items)
    {
        var views = new List<LegacyCareerPathEntryView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyCareerPathEntryView(item));
        }
        return views;
    }

    public static double SumLegacyCareerPathEntryView(LegacyCareerPathEntryView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedQuantity;
    }
}
