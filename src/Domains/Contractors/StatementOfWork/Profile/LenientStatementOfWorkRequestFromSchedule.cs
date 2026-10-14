namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Functions of lenient statement of work request from schedule.</summary>
public static class LenientStatementOfWorkRequestFromSchedule
{
    public static Dictionary<string, double> SummarizeWeightedVendorSchedulesForRequest(List<WeightedVendorSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<LenientStatementOfWorkRequest> DeriveLenientStatementOfWorkRequestsFromSchedule(List<WeightedVendorSchedule> items)
    {
        var outValue = new List<LenientStatementOfWorkRequest>();
        foreach (var item in items)
        {
            var derived = LenientStatementOfWorkRequest.NewLenientStatementOfWorkRequest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectLenientStatementOfWorkRequest(LenientStatementOfWorkRequest item)
    {
        return PrimaryVendorProfilePolicy.NormalizePrimaryVendorProfileCode(item.Code);
    }
}
