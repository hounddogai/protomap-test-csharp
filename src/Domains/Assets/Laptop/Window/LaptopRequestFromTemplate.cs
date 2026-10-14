namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Functions of laptop request from template.</summary>
public static class LaptopRequestFromTemplate
{
    public static Dictionary<string, double> SummarizeFallbackLaptopTemplatesForRequest(List<FallbackLaptopTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<LaptopRequest> DeriveLaptopRequestsFromTemplate(List<FallbackLaptopTemplate> items)
    {
        var outValue = new List<LaptopRequest>();
        foreach (var item in items)
        {
            var derived = LaptopRequest.NewLaptopRequest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanLaptopRequest(LaptopRequest item)
    {
        return LaptopOverridePolicy.NormalizeLaptopOverrideCode(item.Code);
    }
}
