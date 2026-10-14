namespace Acme.Workforce.Assets.Assignment.AssignmentEntry;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Functions of assignment exception from proposal.</summary>
public static class AssignmentExceptionFromProposal
{
    public static Dictionary<string, double> SummarizeScheduledAssetReturnProposalsForException(List<ScheduledAssetReturnProposal> items)
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

    public static List<AssignmentException> DeriveAssignmentExceptionsFromProposal(List<ScheduledAssetReturnProposal> items)
    {
        var outValue = new List<AssignmentException>();
        foreach (var item in items)
        {
            var derived = AssignmentException.NewAssignmentException(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyAssignmentException(AssignmentException item)
    {
        return AssetReturnBaselinePolicy.NormalizeAssetReturnBaselineCode(item.Code);
    }
}
