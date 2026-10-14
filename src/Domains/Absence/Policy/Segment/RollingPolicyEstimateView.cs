namespace Acme.Workforce.Absence.Policy.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling policy estimate.</summary>
public sealed record RollingPolicyEstimateView(string Code, string TitleText, string PriorityText, string Attempts, string SkillText, string Currency, string StatusLabel)
{
    public static RollingPolicyEstimateView ToRollingPolicyEstimateView(RollingPolicyEstimate item)
    {
        return new RollingPolicyEstimateView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 40),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPolicyEstimateView> ToRollingPolicyEstimateViews(List<RollingPolicyEstimate> items)
    {
        var views = new List<RollingPolicyEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPolicyEstimateView(item));
        }
        return views;
    }

    public static double SumRollingPolicyEstimateView(RollingPolicyEstimateView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedAttempts;
    }
}
