namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>Functions of deduction assignment from score.</summary>
public static class DeductionAssignmentFromScore
{
    public static Dictionary<string, double> SummarizeTaxFormScoresForAssignment(List<TaxFormScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<DeductionAssignment> DeriveDeductionAssignmentsFromScore(List<TaxFormScore> items)
    {
        var outValue = new List<DeductionAssignment>();
        foreach (var item in items)
        {
            var derived = DeductionAssignment.NewDeductionAssignment(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyDeductionAssignment(DeductionAssignment item)
    {
        return TaxFormScorePolicy.TaxFormScoreWeight(3, true);
    }
}
