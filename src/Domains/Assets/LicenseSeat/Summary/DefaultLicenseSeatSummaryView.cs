namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default license seat summary.</summary>
public sealed record DefaultLicenseSeatSummaryView(string Code, string Title, string Active, string Weight, string NotesText, string Location, string Segment, string Rate, string Badge, string EmployeeId, string StatusLabel)
{
    public static DefaultLicenseSeatSummaryView ToDefaultLicenseSeatSummaryView(DefaultLicenseSeatSummary item)
    {
        return new DefaultLicenseSeatSummaryView(
            item.Code,
            Text.Truncate(item.Title, 120),
            $"{item.Active}",
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            Text.Truncate(item.Location, 120),
            item.Segment,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultLicenseSeatSummaryView> ToDefaultLicenseSeatSummaryViews(List<DefaultLicenseSeatSummary> items)
    {
        var views = new List<DefaultLicenseSeatSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultLicenseSeatSummaryView(item));
        }
        return views;
    }

    public static double SumDefaultLicenseSeatSummaryView(DefaultLicenseSeatSummaryView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedWeight + parsedRate;
    }
}
