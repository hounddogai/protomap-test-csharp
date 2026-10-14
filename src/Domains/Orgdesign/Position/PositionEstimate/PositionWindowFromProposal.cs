namespace Acme.Workforce.Orgdesign.Position.PositionEstimate;

using Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>Functions of position window from proposal.</summary>
public static class PositionWindowFromProposal
{
    public static Dictionary<string, double> SummarizeHeadcountProposalsForWindow(List<HeadcountProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<PositionWindow> DerivePositionWindowsFromProposal(List<HeadcountProposal> items)
    {
        var outValue = new List<PositionWindow>();
        foreach (var item in items)
        {
            var derived = PositionWindow.NewPositionWindow(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePositionWindow(PositionWindow item)
    {
        return AdaptiveHeadcountAuditPolicy.NormalizeAdaptiveHeadcountAuditCode(item.Code);
    }
}
