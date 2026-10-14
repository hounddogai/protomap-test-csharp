namespace Acme.Workforce.Compensation.PayBand.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a pay band slot.</summary>
public sealed record PayBandSlotView(string Code, string Title, string ThresholdText, string SourceText, string OwnerId, string StatusLabel)
{
    public static PayBandSlotView ToPayBandSlotView(PayBandSlot item)
    {
        return new PayBandSlotView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandSlotView> ToPayBandSlotViews(List<PayBandSlot> items)
    {
        var views = new List<PayBandSlotView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandSlotView(item));
        }
        return views;
    }

    public static double SumPayBandSlotView(PayBandSlotView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
