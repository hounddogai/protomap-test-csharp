namespace Acme.Workforce.Assets.Refresh.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy refresh record.</summary>
public sealed record LegacyRefreshRecordView(string Code, string Label, string CostCenter, string Source, string RateText, string GraceDays, string RatingText, string ContactEmailText, string StatusLabel)
{
    public static LegacyRefreshRecordView ToLegacyRefreshRecordView(LegacyRefreshRecord item)
    {
        return new LegacyRefreshRecordView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.CostCenter,
            item.Source,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyRefreshRecordView> ToLegacyRefreshRecordViews(List<LegacyRefreshRecord> items)
    {
        var views = new List<LegacyRefreshRecordView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyRefreshRecordView(item));
        }
        return views;
    }

    public static double SumLegacyRefreshRecordView(LegacyRefreshRecordView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedGraceDays + parsedRating;
    }
}
