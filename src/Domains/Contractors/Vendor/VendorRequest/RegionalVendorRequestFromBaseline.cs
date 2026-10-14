namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Functions of regional vendor request from baseline.</summary>
public static class RegionalVendorRequestFromBaseline
{
    public static Dictionary<string, double> SummarizeManualStatementOfWorkBaselinesForRequest(List<ManualStatementOfWorkBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<RegionalVendorRequest> DeriveRegionalVendorRequestsFromBaseline(List<ManualStatementOfWorkBaseline> items)
    {
        var outValue = new List<RegionalVendorRequest>();
        foreach (var item in items)
        {
            var derived = RegionalVendorRequest.NewRegionalVendorRequest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateRegionalVendorRequest(RegionalVendorRequest item)
    {
        return PrimaryStatementOfWorkAllocationPolicy.ClassifyPrimaryStatementOfWorkAllocation(item.MeasuredValue());
    }
}
