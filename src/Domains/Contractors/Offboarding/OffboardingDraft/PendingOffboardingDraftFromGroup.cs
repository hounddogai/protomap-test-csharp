namespace Acme.Workforce.Contractors.Offboarding.OffboardingDraft;

using Acme.Workforce.Contractors.StatementOfWork.RecordItem;

/// <summary>Functions of pending offboarding draft from group.</summary>
public static class PendingOffboardingDraftFromGroup
{
    public static Dictionary<string, double> SummarizeManualStatementOfWorkGroupsForDraft(List<ManualStatementOfWorkGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<PendingOffboardingDraft> DerivePendingOffboardingDraftsFromGroup(List<ManualStatementOfWorkGroup> items)
    {
        var outValue = new List<PendingOffboardingDraft>();
        foreach (var item in items)
        {
            var derived = PendingOffboardingDraft.NewPendingOffboardingDraft(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolvePendingOffboardingDraft(PendingOffboardingDraft item)
    {
        return ManualStatementOfWorkGroupPolicy.NormalizeManualStatementOfWorkGroupCode(item.Code);
    }
}
