namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered license seat rule.</summary>
public sealed record TieredLicenseSeatRuleView(string Code, string LabelText, string Limit, string StartsAt, string Approved, string Active, string Ratio, string Phone, string Location, string FloorValue, string StatusLabel)
{
    public static TieredLicenseSeatRuleView ToTieredLicenseSeatRuleView(TieredLicenseSeatRule item)
    {
        return new TieredLicenseSeatRuleView(
            item.Code,
            item.Label,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Active}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            Text.Truncate(item.Location, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredLicenseSeatRuleView> ToTieredLicenseSeatRuleViews(List<TieredLicenseSeatRule> items)
    {
        var views = new List<TieredLicenseSeatRuleView>();
        foreach (var item in items)
        {
            views.Add(ToTieredLicenseSeatRuleView(item));
        }
        return views;
    }

    public static double SumTieredLicenseSeatRuleView(TieredLicenseSeatRuleView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedStartsAt + parsedRatio;
    }
}
