namespace Acme.Workforce.Talent.SkillProfile.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile reservation.</summary>
public sealed record SkillProfileReservationView(string Code, string TitleText, string RetriesText, string Version, string NotesText, string StatusLabel)
{
    public static SkillProfileReservationView ToSkillProfileReservationView(SkillProfileReservation item)
    {
        return new SkillProfileReservationView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileReservationView> ToSkillProfileReservationViews(List<SkillProfileReservation> items)
    {
        var views = new List<SkillProfileReservationView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileReservationView(item));
        }
        return views;
    }

    public static double SumSkillProfileReservationView(SkillProfileReservationView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedVersion;
    }
}
