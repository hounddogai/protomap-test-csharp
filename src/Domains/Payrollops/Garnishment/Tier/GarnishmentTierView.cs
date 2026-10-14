namespace Acme.Workforce.Payrollops.Garnishment.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment tier.</summary>
public sealed record GarnishmentTierView(string Code, string Name, string Unit, string Attempts, string EmployeeId, string ContactEmail, string RegionText, string Pinned, string Badge, string CategoryText, string StatusLabel)
{
    public static GarnishmentTierView ToGarnishmentTierView(GarnishmentTier item)
    {
        return new GarnishmentTierView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Unit,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            Text.Truncate(item.ContactEmail, 64),
            item.Region,
            $"{item.Pinned}",
            Text.Truncate(item.Badge, 120),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentTierView> ToGarnishmentTierViews(List<GarnishmentTier> items)
    {
        var views = new List<GarnishmentTierView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentTierView(item));
        }
        return views;
    }

    public static double SumGarnishmentTierView(GarnishmentTierView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedAttempts;
    }
}
