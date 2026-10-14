namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay tier.</summary>
public sealed record RetroPayTierView(string Code, string Label, string Threshold, string Revision, string ExternalRefText, string Phone, string Unit, string Region, string FloorValue, string StatusLabel)
{
    public static RetroPayTierView ToRetroPayTierView(RetroPayTier item)
    {
        return new RetroPayTierView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            Text.Truncate(item.Phone, 40),
            item.Unit,
            item.Region,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayTierView> ToRetroPayTierViews(List<RetroPayTier> items)
    {
        var views = new List<RetroPayTierView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayTierView(item));
        }
        return views;
    }

    public static double SumRetroPayTierView(RetroPayTierView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedRevision + parsedFloorValue;
    }
}
