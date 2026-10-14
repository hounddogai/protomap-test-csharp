namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Functions of training due proposal from budget.</summary>
public static class TrainingDueProposalFromBudget
{
    public static Dictionary<string, double> SummarizeExceptionBudgetsForProposal(List<ExceptionBudget> items)
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

    public static List<TrainingDueProposal> DeriveTrainingDueProposalsFromBudget(List<ExceptionBudget> items)
    {
        var outValue = new List<TrainingDueProposal>();
        foreach (var item in items)
        {
            var derived = TrainingDueProposal.NewTrainingDueProposal(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectTrainingDueProposal(TrainingDueProposal item)
    {
        return ExceptionMilestonePolicy.ExceptionMilestoneBucket(item.NetValue());
    }
}
