namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict skill profile schedule.</summary>
public sealed record StrictSkillProfileScheduleView(string CodeText, string NameText, string NotesText, string Visible, string JobCode, string PhoneText, string ExternalRef, string Fte, string StatusLabel)
{
    public static StrictSkillProfileScheduleView ToStrictSkillProfileScheduleView(StrictSkillProfileSchedule item)
    {
        return new StrictSkillProfileScheduleView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Notes,
            $"{item.Visible}",
            item.JobCode,
            Text.Truncate(item.Phone, 120),
            item.ExternalRef,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictSkillProfileScheduleView> ToStrictSkillProfileScheduleViews(List<StrictSkillProfileSchedule> items)
    {
        var views = new List<StrictSkillProfileScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToStrictSkillProfileScheduleView(item));
        }
        return views;
    }

    public static double SumStrictSkillProfileScheduleView(StrictSkillProfileScheduleView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
