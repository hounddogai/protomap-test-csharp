namespace Acme.Workforce.Contractors.Rate.Notice;

using System.Globalization;

/// <summary>How screens and exports show a weighted rate notice.</summary>
public sealed record WeightedRateNoticeView(string Code, string Name, string WindowDays, string Checksum, string Retries, string ContactEmail, string Weight, string GraceDays, string Title, string Active, string StatusLabel)
{
    public static WeightedRateNoticeView ToWeightedRateNoticeView(WeightedRateNotice item)
    {
        return new WeightedRateNoticeView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedRateNoticeView> ToWeightedRateNoticeViews(List<WeightedRateNotice> items)
    {
        var views = new List<WeightedRateNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedRateNoticeView(item));
        }
        return views;
    }

    public static double SumWeightedRateNoticeView(WeightedRateNoticeView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedRetries + parsedWeight;
    }
}
