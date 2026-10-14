namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using System.Globalization;

/// <summary>How screens and exports show a license seat record.</summary>
public sealed record LicenseSeatRecordView(string Code, string Name, string Score, string Phone, string BatchSize, string WindowDays, string StatusLabel)
{
    public static LicenseSeatRecordView ToLicenseSeatRecordView(LicenseSeatRecord item)
    {
        return new LicenseSeatRecordView(
            item.Code,
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatRecordView> ToLicenseSeatRecordViews(List<LicenseSeatRecord> items)
    {
        var views = new List<LicenseSeatRecordView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatRecordView(item));
        }
        return views;
    }

    public static double SumLicenseSeatRecordView(LicenseSeatRecordView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedBatchSize + (double)parsedWindowDays;
    }
}
