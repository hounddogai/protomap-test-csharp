namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Assets.LicenseSeat.Template;

/// <summary>Functions of laptop request from window.</summary>
public static class LaptopRequestFromWindow
{
    public static Dictionary<string, double> SummarizeLegacyLicenseSeatWindowsForRequest(List<LegacyLicenseSeatWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<LaptopRequest> DeriveLaptopRequestsFromWindow(List<LegacyLicenseSeatWindow> items)
    {
        var outValue = new List<LaptopRequest>();
        foreach (var item in items)
        {
            var derived = LaptopRequest.NewLaptopRequest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateLaptopRequest(LaptopRequest item)
    {
        return LicenseSeatTemplatePolicy.LicenseSeatTemplateWeight(3, true);
    }
}
