namespace Acme.Workforce.Compensation.PayBand.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched pay band mapping.</summary>
public sealed record BatchedPayBandMappingView(string Code, string Name, string Capacity, string Description, string Team, string Threshold, string PhoneText, string EndsAt, string StatusLabel)
{
    public static BatchedPayBandMappingView ToBatchedPayBandMappingView(BatchedPayBandMapping item)
    {
        return new BatchedPayBandMappingView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.Team, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedPayBandMappingView> ToBatchedPayBandMappingViews(List<BatchedPayBandMapping> items)
    {
        var views = new List<BatchedPayBandMappingView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedPayBandMappingView(item));
        }
        return views;
    }

    public static double SumBatchedPayBandMappingView(BatchedPayBandMappingView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedThreshold + (double)parsedEndsAt;
    }
}
