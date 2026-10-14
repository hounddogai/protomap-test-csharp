namespace Acme.Workforce.Compensation.PayBand.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band mapping.</summary>
public sealed record PayBandMappingView(string Code, string Label, string TitleText, string RevisionText, string Limit, string BatchSize, string Threshold, string StartsAtText, string Phone, string Name, string GraceDays, string StatusLabel)
{
    public static PayBandMappingView ToPayBandMappingView(PayBandMapping item)
    {
        return new PayBandMappingView(
            item.Code,
            item.Label,
            item.Title,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 40),
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandMappingView> ToPayBandMappingViews(List<PayBandMapping> items)
    {
        var views = new List<PayBandMappingView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandMappingView(item));
        }
        return views;
    }

    public static double SumPayBandMappingView(PayBandMappingView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedLimit + (double)parsedBatchSize;
    }
}
