namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Functions of nomination ledger from window.</summary>
public static class NominationLedgerFromWindow
{
    public static Dictionary<string, double> SummarizeCappedNominationWindowsForLedger(List<CappedNominationWindow> items)
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

    public static List<NominationLedger> DeriveNominationLedgersFromWindow(List<CappedNominationWindow> items)
    {
        var outValue = new List<NominationLedger>();
        foreach (var item in items)
        {
            var derived = NominationLedger.NewNominationLedger(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeNominationLedger(NominationLedger item)
    {
        return NominationSlotPolicy.ClassifyNominationSlot(item.AdjustedExposure());
    }
}
