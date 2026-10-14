namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat segment.</summary>
public sealed record LicenseSeatSegmentView(string Code, string Label, string Pinned, string Notes, string CurrencyText, string Archived, string Hours, string StatusLabel)
{
    public static LicenseSeatSegmentView ToLicenseSeatSegmentView(LicenseSeatSegment item)
    {
        return new LicenseSeatSegmentView(
            item.Code,
            Text.Truncate(item.Label, 64),
            $"{item.Pinned}",
            item.Notes,
            item.Currency,
            $"{item.Archived}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatSegmentView> ToLicenseSeatSegmentViews(List<LicenseSeatSegment> items)
    {
        var views = new List<LicenseSeatSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatSegmentView(item));
        }
        return views;
    }

    public static double SumLicenseSeatSegmentView(LicenseSeatSegmentView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedHours;
    }
}
