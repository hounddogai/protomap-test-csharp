namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Compliance.Exception.Proposal;

/// <summary>Functions of training due rollup from proposal.</summary>
public static class TrainingDueRollupFromProposal
{
    public static Dictionary<string, double> SummarizeExceptionProposalsForRollup(List<ExceptionProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<TrainingDueRollup> DeriveTrainingDueRollupsFromProposal(List<ExceptionProposal> items)
    {
        var outValue = new List<TrainingDueRollup>();
        foreach (var item in items)
        {
            var derived = TrainingDueRollup.NewTrainingDueRollup(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeTrainingDueRollup(TrainingDueRollup item)
    {
        return ExceptionOverridePolicy.NormalizeExceptionOverrideCode(item.Code);
    }
}
