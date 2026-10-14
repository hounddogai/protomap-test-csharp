namespace Acme.Workforce.Talent.SkillProfile.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled skill profile policy.</summary>
public sealed record ScheduledSkillProfilePolicyView(string CodeText, string LabelText, string RateText, string GraceDays, string SegmentText, string Ceiling, string JobCode, string UpdatedAt, string Maximum, string TenureMonthsText, string StatusLabel)
{
    public static ScheduledSkillProfilePolicyView ToScheduledSkillProfilePolicyView(ScheduledSkillProfilePolicy item)
    {
        return new ScheduledSkillProfilePolicyView(
            item.Code,
            item.Label,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 40),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledSkillProfilePolicyView> ToScheduledSkillProfilePolicyViews(List<ScheduledSkillProfilePolicy> items)
    {
        var views = new List<ScheduledSkillProfilePolicyView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledSkillProfilePolicyView(item));
        }
        return views;
    }

    public static double SumScheduledSkillProfilePolicyView(ScheduledSkillProfilePolicyView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedGraceDays + parsedCeiling;
    }
}
