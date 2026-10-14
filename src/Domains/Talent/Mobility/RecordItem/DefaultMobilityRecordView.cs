namespace Acme.Workforce.Talent.Mobility.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default mobility record.</summary>
public sealed record DefaultMobilityRecordView(string CodeText, string Label, string WindowDaysText, string Archived, string Margin, string Segment, string RatioText, string NotesText, string StartsAt, string Currency, string SalaryBandText, string StatusLabel)
{
    public static DefaultMobilityRecordView ToDefaultMobilityRecordView(DefaultMobilityRecord item)
    {
        return new DefaultMobilityRecordView(
            item.Code,
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 40),
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultMobilityRecordView> ToDefaultMobilityRecordViews(List<DefaultMobilityRecord> items)
    {
        var views = new List<DefaultMobilityRecordView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultMobilityRecordView(item));
        }
        return views;
    }

    public static double SumDefaultMobilityRecordView(DefaultMobilityRecordView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedMargin + parsedRatio;
    }
}
