namespace Acme.Workforce.Assets.LicenseSeat.SeatReservation;

using Acme.Workforce.Assets.Refresh.Mapping;

/// <summary>Functions of license seat window from manifest.</summary>
public static class LicenseSeatWindowFromManifest
{
    public static Dictionary<string, double> SummarizeRollingRefreshManifestsForWindow(List<RollingRefreshManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<LicenseSeatWindow> DeriveLicenseSeatWindowsFromManifest(List<RollingRefreshManifest> items)
    {
        var outValue = new List<LicenseSeatWindow>();
        foreach (var item in items)
        {
            var derived = LicenseSeatWindow.NewLicenseSeatWindow(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectLicenseSeatWindow(LicenseSeatWindow item)
    {
        return ScheduledRefreshMappingPolicy.ClassifyScheduledRefreshMapping(item.WeightedExposure());
    }
}
