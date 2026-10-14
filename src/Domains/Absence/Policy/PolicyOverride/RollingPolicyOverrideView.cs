namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling policy override.</summary>
public sealed record RollingPolicyOverrideView(string Code, string Name, string Country, string Quantity, string Segment, string DurationMinutesText, string Budget, string ExternalRef, string Badge, string StartsAt, string EndsAt, string StatusLabel)
{
    public static RollingPolicyOverrideView ToRollingPolicyOverrideView(RollingPolicyOverride item)
    {
        return new RollingPolicyOverrideView(
            item.Code,
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.Country, 80),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Badge,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPolicyOverrideView> ToRollingPolicyOverrideViews(List<RollingPolicyOverride> items)
    {
        var views = new List<RollingPolicyOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPolicyOverrideView(item));
        }
        return views;
    }

    public static double SumRollingPolicyOverrideView(RollingPolicyOverrideView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedDurationMinutes + parsedBudget;
    }
}
