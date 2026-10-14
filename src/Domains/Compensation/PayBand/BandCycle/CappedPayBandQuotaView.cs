namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using System.Globalization;

/// <summary>How screens and exports show a capped pay band quota.</summary>
public sealed record CappedPayBandQuotaView(string Code, string Title, string FloorValue, string Department, string EmployeeId, string StatusLabel)
{
    public static CappedPayBandQuotaView ToCappedPayBandQuotaView(CappedPayBandQuota item)
    {
        return new CappedPayBandQuotaView(
            item.Code,
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPayBandQuotaView> ToCappedPayBandQuotaViews(List<CappedPayBandQuota> items)
    {
        var views = new List<CappedPayBandQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPayBandQuotaView(item));
        }
        return views;
    }

    public static double SumCappedPayBandQuotaView(CappedPayBandQuotaView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedFloorValue;
    }
}
