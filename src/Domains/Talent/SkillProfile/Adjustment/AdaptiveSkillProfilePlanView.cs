namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive skill profile plan.</summary>
public sealed record AdaptiveSkillProfilePlanView(string CodeText, string NameText, string DurationMinutes, string Capacity, string Description, string Notes, string EmployeeId, string StatusLabel)
{
    public static AdaptiveSkillProfilePlanView ToAdaptiveSkillProfilePlanView(AdaptiveSkillProfilePlan item)
    {
        return new AdaptiveSkillProfilePlanView(
            item.Code,
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.Notes, 64),
            Text.Truncate(item.EmployeeId, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveSkillProfilePlanView> ToAdaptiveSkillProfilePlanViews(List<AdaptiveSkillProfilePlan> items)
    {
        var views = new List<AdaptiveSkillProfilePlanView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveSkillProfilePlanView(item));
        }
        return views;
    }

    public static double SumAdaptiveSkillProfilePlanView(AdaptiveSkillProfilePlanView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedCapacity;
    }
}
