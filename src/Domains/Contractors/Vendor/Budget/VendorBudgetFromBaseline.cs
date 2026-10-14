namespace Acme.Workforce.Contractors.Vendor.Budget;

using Acme.Workforce.Contractors.Timesheet.Summary;

/// <summary>Functions of vendor budget from baseline.</summary>
public static class VendorBudgetFromBaseline
{
    public static Dictionary<string, double> SummarizeTimesheetBaselinesForBudget(List<TimesheetBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<VendorBudget> DeriveVendorBudgetsFromBaseline(List<TimesheetBaseline> items)
    {
        var outValue = new List<VendorBudget>();
        foreach (var item in items)
        {
            var derived = VendorBudget.NewVendorBudget(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyVendorBudget(VendorBudget item)
    {
        return TimesheetSummaryPolicy.TimesheetSummaryWeight(1, true);
    }
}
