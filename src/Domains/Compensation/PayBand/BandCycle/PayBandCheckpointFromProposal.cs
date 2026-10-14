namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Compensation.MeritCycle.Metric;

/// <summary>Functions of pay band checkpoint from proposal.</summary>
public static class PayBandCheckpointFromProposal
{
    public static Dictionary<string, double> SummarizeManualMeritCycleProposalsForCheckpoint(List<ManualMeritCycleProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<PayBandCheckpoint> DerivePayBandCheckpointsFromProposal(List<ManualMeritCycleProposal> items)
    {
        var outValue = new List<PayBandCheckpoint>();
        foreach (var item in items)
        {
            var derived = PayBandCheckpoint.NewPayBandCheckpoint(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizePayBandCheckpoint(PayBandCheckpoint item)
    {
        return CappedMeritCycleDigestPolicy.NormalizeCappedMeritCycleDigestCode(item.Code);
    }
}
