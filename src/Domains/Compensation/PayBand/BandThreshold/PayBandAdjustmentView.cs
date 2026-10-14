namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band adjustment.</summary>
public sealed record PayBandAdjustmentView(string Code, string Label, string ExternalRefText, string Fte, string Rate, string StatusLabel)
{
    public static PayBandAdjustmentView ToPayBandAdjustmentView(PayBandAdjustment item)
    {
        return new PayBandAdjustmentView(
            item.Code,
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.ExternalRef, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandAdjustmentView> ToPayBandAdjustmentViews(List<PayBandAdjustment> items)
    {
        var views = new List<PayBandAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandAdjustmentView(item));
        }
        return views;
    }

    public static double SumPayBandAdjustmentView(PayBandAdjustmentView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedFte + parsedRate;
    }
}
