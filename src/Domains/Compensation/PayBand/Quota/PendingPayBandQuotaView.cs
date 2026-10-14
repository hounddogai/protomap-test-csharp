namespace Acme.Workforce.Compensation.PayBand.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending pay band quota.</summary>
public sealed record PendingPayBandQuotaView(string Code, string Name, string Weight, string Minimum, string Source, string StatusLabel)
{
    public static PendingPayBandQuotaView ToPendingPayBandQuotaView(PendingPayBandQuota item)
    {
        return new PendingPayBandQuotaView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPayBandQuotaView> ToPendingPayBandQuotaViews(List<PendingPayBandQuota> items)
    {
        var views = new List<PendingPayBandQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPayBandQuotaView(item));
        }
        return views;
    }

    public static double SumPendingPayBandQuotaView(PendingPayBandQuotaView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedWeight + parsedMinimum;
    }
}
