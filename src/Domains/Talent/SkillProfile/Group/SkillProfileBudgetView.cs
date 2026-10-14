namespace Acme.Workforce.Talent.SkillProfile.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile budget.</summary>
public sealed record SkillProfileBudgetView(string CodeText, string Title, string ApprovedText, string ThresholdText, string JobCodeText, string ManagerId, string GraceDays, string EmployeeId, string Retries, string StatusLabel)
{
    public static SkillProfileBudgetView ToSkillProfileBudgetView(SkillProfileBudget item)
    {
        return new SkillProfileBudgetView(
            item.Code,
            Text.Truncate(item.Title, 120),
            $"{item.Approved}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            Text.Truncate(item.ManagerId, 40),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileBudgetView> ToSkillProfileBudgetViews(List<SkillProfileBudget> items)
    {
        var views = new List<SkillProfileBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileBudgetView(item));
        }
        return views;
    }

    public static double SumSkillProfileBudgetView(SkillProfileBudgetView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedGraceDays + (double)parsedRetries;
    }
}
