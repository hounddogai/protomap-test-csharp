namespace Acme.Workforce.Talent.SkillProfile.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional skill profile group.</summary>
public sealed record RegionalSkillProfileGroupView(string Code, string Label, string BudgetText, string Grade, string Channel, string JobCode, string DurationMinutes, string StatusLabel)
{
    public static RegionalSkillProfileGroupView ToRegionalSkillProfileGroupView(RegionalSkillProfileGroup item)
    {
        return new RegionalSkillProfileGroupView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 64),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Channel,
            Text.Truncate(item.JobCode, 64),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalSkillProfileGroupView> ToRegionalSkillProfileGroupViews(List<RegionalSkillProfileGroup> items)
    {
        var views = new List<RegionalSkillProfileGroupView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalSkillProfileGroupView(item));
        }
        return views;
    }

    public static double SumRegionalSkillProfileGroupView(RegionalSkillProfileGroupView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedDurationMinutes;
    }
}
