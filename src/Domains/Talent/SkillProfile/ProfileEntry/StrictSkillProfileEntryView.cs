namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict skill profile entry.</summary>
public sealed record StrictSkillProfileEntryView(string Code, string Title, string SalaryBandText, string EmployeeId, string Fte, string Checksum, string Team, string Archived, string Active, string StatusLabel)
{
    public static StrictSkillProfileEntryView ToStrictSkillProfileEntryView(StrictSkillProfileEntry item)
    {
        return new StrictSkillProfileEntryView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.SalaryBand,
            item.EmployeeId,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.Team,
            $"{item.Archived}",
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictSkillProfileEntryView> ToStrictSkillProfileEntryViews(List<StrictSkillProfileEntry> items)
    {
        var views = new List<StrictSkillProfileEntryView>();
        foreach (var item in items)
        {
            views.Add(ToStrictSkillProfileEntryView(item));
        }
        return views;
    }

    public static double SumStrictSkillProfileEntryView(StrictSkillProfileEntryView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
