namespace Acme.Workforce.Talent.SkillProfile.Entry;

using System.Globalization;

/// <summary>How screens and exports show a skill profile entry.</summary>
public sealed record SkillProfileEntryView(string Code, string Label, string LimitText, string Archived, string CapacityText, string StatusLabel)
{
    public static SkillProfileEntryView ToSkillProfileEntryView(SkillProfileEntry item)
    {
        return new SkillProfileEntryView(
            item.Code,
            item.Label,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileEntryView> ToSkillProfileEntryViews(List<SkillProfileEntry> items)
    {
        var views = new List<SkillProfileEntryView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileEntryView(item));
        }
        return views;
    }

    public static double SumSkillProfileEntryView(SkillProfileEntryView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedCapacity;
    }
}
