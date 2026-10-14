namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>Functions of primary license seat window from template.</summary>
public static class PrimaryLicenseSeatWindowFromTemplate
{
    public static Dictionary<string, double> SummarizeRefreshTemplatesForWindow(List<RefreshTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<PrimaryLicenseSeatWindow> DerivePrimaryLicenseSeatWindowsFromTemplate(List<RefreshTemplate> items)
    {
        var outValue = new List<PrimaryLicenseSeatWindow>();
        foreach (var item in items)
        {
            var derived = PrimaryLicenseSeatWindow.NewPrimaryLicenseSeatWindow(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalancePrimaryLicenseSeatWindow(PrimaryLicenseSeatWindow item)
    {
        return RefreshTemplatePolicy.NormalizeRefreshTemplateCode(item.Code);
    }
}
