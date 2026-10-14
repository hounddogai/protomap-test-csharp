namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling equity grant digest.</summary>
public sealed record RollingEquityGrantDigestView(string CodeText, string LabelText, string Limit, string SummaryText, string CurrencyText, string Category, string TenureMonths, string Name, string LeaveDays, string StatusLabel)
{
    public static RollingEquityGrantDigestView ToRollingEquityGrantDigestView(RollingEquityGrantDigest item)
    {
        return new RollingEquityGrantDigestView(
            item.Code,
            item.Label,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            Text.Truncate(item.Currency, 64),
            item.Category,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingEquityGrantDigestView> ToRollingEquityGrantDigestViews(List<RollingEquityGrantDigest> items)
    {
        var views = new List<RollingEquityGrantDigestView>();
        foreach (var item in items)
        {
            views.Add(ToRollingEquityGrantDigestView(item));
        }
        return views;
    }

    public static double SumRollingEquityGrantDigestView(RollingEquityGrantDigestView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedTenureMonths + (double)parsedLeaveDays;
    }
}
