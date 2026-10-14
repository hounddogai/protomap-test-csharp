namespace Acme.Workforce.Talent.SkillProfile.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile record.</summary>
public sealed record SkillProfileRecordView(string Code, string Name, string Pinned, string LeaveDays, string LocationText, string Channel, string Summary, string ManagerIdText, string Budget, string JobCodeText, string Segment, string StatusLabel)
{
    public static SkillProfileRecordView ToSkillProfileRecordView(SkillProfileRecord item)
    {
        return new SkillProfileRecordView(
            item.Code,
            Text.Truncate(item.Name, 64),
            $"{item.Pinned}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Channel,
            item.Summary,
            item.ManagerId,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 64),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileRecordView> ToSkillProfileRecordViews(List<SkillProfileRecord> items)
    {
        var views = new List<SkillProfileRecordView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileRecordView(item));
        }
        return views;
    }

    public static double SumSkillProfileRecordView(SkillProfileRecordView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedBudget;
    }
}
