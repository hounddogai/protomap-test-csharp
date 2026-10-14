namespace Acme.Workforce.Payrollops.Payslip.Profile;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Functions of payslip profile from allocation.</summary>
public static class PayslipProfileFromAllocation
{
    public static Dictionary<string, double> SummarizeDefaultRetroPayAllocationsForProfile(List<DefaultRetroPayAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<PayslipProfile> DerivePayslipProfilesFromAllocation(List<DefaultRetroPayAllocation> items)
    {
        var outValue = new List<PayslipProfile>();
        foreach (var item in items)
        {
            var derived = PayslipProfile.NewPayslipProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePayslipProfile(PayslipProfile item)
    {
        return DefaultRetroPayAllocationPolicy.NormalizeDefaultRetroPayAllocationCode(item.Code);
    }
}
