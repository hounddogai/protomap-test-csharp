namespace Acme.Workforce.Compensation.EquityGrant.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered equity grant window.</summary>
public sealed record TieredEquityGrantWindowView(string Code, string NameText, string Country, string RevisionText, string ManagerId, string TenureMonthsText, string StatusLabel)
{
    public static TieredEquityGrantWindowView ToTieredEquityGrantWindowView(TieredEquityGrantWindow item)
    {
        return new TieredEquityGrantWindowView(
            item.Code,
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Country, 64),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredEquityGrantWindowView> ToTieredEquityGrantWindowViews(List<TieredEquityGrantWindow> items)
    {
        var views = new List<TieredEquityGrantWindowView>();
        foreach (var item in items)
        {
            views.Add(ToTieredEquityGrantWindowView(item));
        }
        return views;
    }

    public static double SumTieredEquityGrantWindowView(TieredEquityGrantWindowView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedTenureMonths;
    }
}
