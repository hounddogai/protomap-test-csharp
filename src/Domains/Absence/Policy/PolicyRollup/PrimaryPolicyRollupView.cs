namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary policy rollup.</summary>
public sealed record PrimaryPolicyRollupView(string Code, string LabelText, string SkillText, string Limit, string Budget, string StatusLabel)
{
    public static PrimaryPolicyRollupView ToPrimaryPolicyRollupView(PrimaryPolicyRollup item)
    {
        return new PrimaryPolicyRollupView(
            item.Code,
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.Skill, 64),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPolicyRollupView> ToPrimaryPolicyRollupViews(List<PrimaryPolicyRollup> items)
    {
        var views = new List<PrimaryPolicyRollupView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPolicyRollupView(item));
        }
        return views;
    }

    public static double SumPrimaryPolicyRollupView(PrimaryPolicyRollupView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedBudget;
    }
}
