namespace Acme.Workforce.Absence.Policy.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy item.</summary>
public sealed record PolicyItemView(string Code, string Label, string Currency, string Visible, string Team, string Locale, string CategoryText, string DiscountText, string UpdatedAtText, string ExternalRef, string StatusLabel)
{
    public static PolicyItemView ToPolicyItemView(PolicyItem item)
    {
        return new PolicyItemView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Currency,
            $"{item.Visible}",
            item.Team,
            item.Locale,
            Text.Truncate(item.Category, 80),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyItemView> ToPolicyItemViews(List<PolicyItem> items)
    {
        var views = new List<PolicyItemView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyItemView(item));
        }
        return views;
    }

    public static double SumPolicyItemView(PolicyItemView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedUpdatedAt;
    }
}
