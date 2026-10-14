namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Functions of lenient leave request agreement from plan.</summary>
public static class LenientLeaveRequestAgreementFromPlan
{
    public static Dictionary<string, double> SummarizeAccrualPlansForAgreement(List<AccrualPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<LenientLeaveRequestAgreement> DeriveLenientLeaveRequestAgreementsFromPlan(List<AccrualPlan> items)
    {
        var outValue = new List<LenientLeaveRequestAgreement>();
        foreach (var item in items)
        {
            var derived = LenientLeaveRequestAgreement.NewLenientLeaveRequestAgreement(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignLenientLeaveRequestAgreement(LenientLeaveRequestAgreement item)
    {
        return CappedAccrualManifestPolicy.CappedAccrualManifestWeight(3, true);
    }
}
