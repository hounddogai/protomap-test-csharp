namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Functions of manual laptop bundle from proposal.</summary>
public static class ManualLaptopBundleFromProposal
{
    public static Dictionary<string, double> SummarizeScheduledAssetReturnProposalsForBundle(List<ScheduledAssetReturnProposal> items)
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

    public static List<ManualLaptopBundle> DeriveManualLaptopBundlesFromProposal(List<ScheduledAssetReturnProposal> items)
    {
        var outValue = new List<ManualLaptopBundle>();
        foreach (var item in items)
        {
            var derived = ManualLaptopBundle.NewManualLaptopBundle(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectManualLaptopBundle(ManualLaptopBundle item)
    {
        return ScheduledAssetReturnProposalPolicy.NormalizeScheduledAssetReturnProposalCode(item.Code);
    }
}
