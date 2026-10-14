namespace Acme.Workforce.Absence.Accrual.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual segment.</summary>
public sealed record AccrualSegmentView(string Code, string NameText, string Source, string Minimum, string Currency, string Margin, string Grade, string Notes, string Ratio, string ContactEmail, string StatusLabel)
{
    public static AccrualSegmentView ToAccrualSegmentView(AccrualSegment item)
    {
        return new AccrualSegmentView(
            item.Code,
            item.Name,
            item.Source,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            Text.Truncate(item.Notes, 40),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualSegmentView> ToAccrualSegmentViews(List<AccrualSegment> items)
    {
        var views = new List<AccrualSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualSegmentView(item));
        }
        return views;
    }

    public static double SumAccrualSegmentView(AccrualSegmentView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedMargin + parsedRatio;
    }
}
