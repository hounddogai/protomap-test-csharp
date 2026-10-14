namespace Acme.Workforce.Assets.Refresh.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh segment.</summary>
public sealed record RefreshSegmentView(string Code, string Title, string Team, string SalaryBandText, string DiscountText, string Sequence, string StatusLabel)
{
    public static RefreshSegmentView ToRefreshSegmentView(RefreshSegment item)
    {
        return new RefreshSegmentView(
            item.Code,
            item.Title,
            Text.Truncate(item.Team, 64),
            item.SalaryBand,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshSegmentView> ToRefreshSegmentViews(List<RefreshSegment> items)
    {
        var views = new List<RefreshSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshSegmentView(item));
        }
        return views;
    }

    public static double SumRefreshSegmentView(RefreshSegmentView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedSequence;
    }
}
