namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Assets.Assignment.Snapshot;

/// <summary>Functions of legacy license seat proposal from snapshot.</summary>
public static class LegacyLicenseSeatProposalFromSnapshot
{
    public static Dictionary<string, double> SummarizeDefaultAssignmentSnapshotsForProposal(List<DefaultAssignmentSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<LegacyLicenseSeatProposal> DeriveLegacyLicenseSeatProposalsFromSnapshot(List<DefaultAssignmentSnapshot> items)
    {
        var outValue = new List<LegacyLicenseSeatProposal>();
        foreach (var item in items)
        {
            var derived = LegacyLicenseSeatProposal.NewLegacyLicenseSeatProposal(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLegacyLicenseSeatProposalExtra(LegacyLicenseSeatProposal item)
    {
        return DefaultAssignmentSnapshotPolicy.ClassifyDefaultAssignmentSnapshot(item.EffectiveTotal());
    }
}
