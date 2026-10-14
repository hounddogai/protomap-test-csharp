namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending equity grant override.</summary>
public sealed record PendingEquityGrantOverrideView(string Code, string Name, string Discount, string ExternalRefText, string Category, string StatusLabel)
{
    public static PendingEquityGrantOverrideView ToPendingEquityGrantOverrideView(PendingEquityGrantOverride item)
    {
        return new PendingEquityGrantOverrideView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingEquityGrantOverrideView> ToPendingEquityGrantOverrideViews(List<PendingEquityGrantOverride> items)
    {
        var views = new List<PendingEquityGrantOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToPendingEquityGrantOverrideView(item));
        }
        return views;
    }

    public static double SumPendingEquityGrantOverrideView(PendingEquityGrantOverrideView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedDiscount;
    }
}
