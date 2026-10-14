namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual retro pay tier.</summary>
public sealed record ManualRetroPayTierView(string CodeText, string Name, string FloorValue, string Team, string ManagerId, string Version, string StatusLabel)
{
    public static ManualRetroPayTierView ToManualRetroPayTierView(ManualRetroPayTier item)
    {
        return new ManualRetroPayTierView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.ManagerId,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualRetroPayTierView> ToManualRetroPayTierViews(List<ManualRetroPayTier> items)
    {
        var views = new List<ManualRetroPayTierView>();
        foreach (var item in items)
        {
            views.Add(ToManualRetroPayTierView(item));
        }
        return views;
    }

    public static double SumManualRetroPayTierView(ManualRetroPayTierView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedVersion;
    }
}
