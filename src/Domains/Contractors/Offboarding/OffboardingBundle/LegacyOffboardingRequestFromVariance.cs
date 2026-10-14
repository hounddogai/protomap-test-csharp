namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Functions of legacy offboarding request from variance.</summary>
public static class LegacyOffboardingRequestFromVariance
{
    public static Dictionary<string, double> SummarizeScheduledStatementOfWorkVariancesForRequest(List<ScheduledStatementOfWorkVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<LegacyOffboardingRequest> DeriveLegacyOffboardingRequestsFromVariance(List<ScheduledStatementOfWorkVariance> items)
    {
        var outValue = new List<LegacyOffboardingRequest>();
        foreach (var item in items)
        {
            var derived = LegacyOffboardingRequest.NewLegacyOffboardingRequest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectLegacyOffboardingRequest(LegacyOffboardingRequest item)
    {
        return PrimaryStatementOfWorkAllocationPolicy.ClassifyPrimaryStatementOfWorkAllocation(item.ProjectedVolume());
    }
}
