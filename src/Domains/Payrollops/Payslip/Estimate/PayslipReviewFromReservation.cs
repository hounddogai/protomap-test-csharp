namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Functions of payslip review from reservation.</summary>
public static class PayslipReviewFromReservation
{
    public static Dictionary<string, double> SummarizeDeductionReservationsForReview(List<DeductionReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PayslipReview> DerivePayslipReviewsFromReservation(List<DeductionReservation> items)
    {
        var outValue = new List<PayslipReview>();
        foreach (var item in items)
        {
            var derived = PayslipReview.NewPayslipReview(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanPayslipReview(PayslipReview item)
    {
        return DeductionBudgetPolicy.DeductionBudgetWeight(8, true);
    }
}
