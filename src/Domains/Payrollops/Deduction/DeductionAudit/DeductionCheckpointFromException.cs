namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Functions of deduction checkpoint from exception.</summary>
public static class DeductionCheckpointFromException
{
    public static Dictionary<string, double> SummarizeTaxFormExceptionsForCheckpoint(List<TaxFormException> items)
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

    public static List<DeductionCheckpoint> DeriveDeductionCheckpointsFromException(List<TaxFormException> items)
    {
        var outValue = new List<DeductionCheckpoint>();
        foreach (var item in items)
        {
            var derived = DeductionCheckpoint.NewDeductionCheckpoint(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeDeductionCheckpoint(DeductionCheckpoint item)
    {
        return TaxFormExceptionPolicy.NormalizeTaxFormExceptionCode(item.Code);
    }
}
