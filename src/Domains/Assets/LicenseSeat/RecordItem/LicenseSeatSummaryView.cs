namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat summary.</summary>
public sealed record LicenseSeatSummaryView(string Code, string Title, string Discount, string Cost, string Notes, string StatusLabel)
{
    public static LicenseSeatSummaryView ToLicenseSeatSummaryView(LicenseSeatSummary item)
    {
        return new LicenseSeatSummaryView(
            item.Code,
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatSummaryView> ToLicenseSeatSummaryViews(List<LicenseSeatSummary> items)
    {
        var views = new List<LicenseSeatSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatSummaryView(item));
        }
        return views;
    }

    public static double SumLicenseSeatSummaryView(LicenseSeatSummaryView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedCost;
    }
}
