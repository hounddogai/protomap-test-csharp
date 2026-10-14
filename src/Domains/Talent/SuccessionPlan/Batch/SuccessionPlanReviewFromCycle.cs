namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

/// <summary>Functions of succession plan review from cycle.</summary>
public static class SuccessionPlanReviewFromCycle
{
    public static Dictionary<string, double> SummarizeCappedSuccessionPlanCyclesForReview(List<CappedSuccessionPlanCycle> items)
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

    public static List<SuccessionPlanReview> DeriveSuccessionPlanReviewsFromCycle(List<CappedSuccessionPlanCycle> items)
    {
        var outValue = new List<SuccessionPlanReview>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanReview.NewSuccessionPlanReview(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveSuccessionPlanReview(SuccessionPlanReview item)
    {
        return LenientSuccessionPlanAgreementPolicy.NormalizeLenientSuccessionPlanAgreementCode(item.Code);
    }
}
