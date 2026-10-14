namespace Acme.Workforce.Assets.Laptop.Rollup;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Functions of laptop proposal from mapping.</summary>
public static class LaptopProposalFromMapping
{
    public static Dictionary<string, double> SummarizeRollingLaptopMappingsForProposal(List<RollingLaptopMapping> items)
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

    public static List<LaptopProposal> DeriveLaptopProposalsFromMapping(List<RollingLaptopMapping> items)
    {
        var outValue = new List<LaptopProposal>();
        foreach (var item in items)
        {
            var derived = LaptopProposal.NewLaptopProposal(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignLaptopProposal(LaptopProposal item)
    {
        return ManualLaptopExceptionPolicy.NormalizeManualLaptopExceptionCode(item.Code);
    }
}
