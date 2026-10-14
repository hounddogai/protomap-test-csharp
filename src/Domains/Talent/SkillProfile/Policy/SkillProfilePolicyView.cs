namespace Acme.Workforce.Talent.SkillProfile.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile policy.</summary>
public sealed record SkillProfilePolicyView(string Code, string Name, string GraceDays, string Priority, string Amount, string JobCode, string ContactEmail, string Skill, string StatusLabel)
{
    public static SkillProfilePolicyView ToSkillProfilePolicyView(SkillProfilePolicy item)
    {
        return new SkillProfilePolicyView(
            item.Code,
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.ContactEmail,
            Text.Truncate(item.Skill, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfilePolicyView> ToSkillProfilePolicyViews(List<SkillProfilePolicy> items)
    {
        var views = new List<SkillProfilePolicyView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfilePolicyView(item));
        }
        return views;
    }

    public static double SumSkillProfilePolicyView(SkillProfilePolicyView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedPriority + parsedAmount;
    }
}
