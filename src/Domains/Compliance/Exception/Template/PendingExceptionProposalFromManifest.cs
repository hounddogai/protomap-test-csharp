namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Functions of pending exception proposal from manifest.</summary>
public static class PendingExceptionProposalFromManifest
{
    public static Dictionary<string, double> SummarizePendingExceptionManifestsForProposal(List<PendingExceptionManifest> items)
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

    public static List<PendingExceptionProposal> DerivePendingExceptionProposalsFromManifest(List<PendingExceptionManifest> items)
    {
        var outValue = new List<PendingExceptionProposal>();
        foreach (var item in items)
        {
            var derived = PendingExceptionProposal.NewPendingExceptionProposal(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPendingExceptionProposal(PendingExceptionProposal item)
    {
        return ExceptionRecordPolicy.NormalizeExceptionRecordCode(item.Code);
    }
}
