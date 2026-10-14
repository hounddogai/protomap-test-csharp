namespace Acme.Workforce.Assets.Assignment.Rollup;

using Acme.Workforce.Assets.LicenseSeat.Template;

/// <summary>Functions of assignment draft from template.</summary>
public static class AssignmentDraftFromTemplate
{
    public static Dictionary<string, double> SummarizeLicenseSeatTemplatesForDraft(List<LicenseSeatTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<AssignmentDraft> DeriveAssignmentDraftsFromTemplate(List<LicenseSeatTemplate> items)
    {
        var outValue = new List<AssignmentDraft>();
        foreach (var item in items)
        {
            var derived = AssignmentDraft.NewAssignmentDraft(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleAssignmentDraft(AssignmentDraft item)
    {
        return PendingLicenseSeatCheckpointPolicy.ClassifyPendingLicenseSeatCheckpoint(item.EffectiveValue());
    }
}
