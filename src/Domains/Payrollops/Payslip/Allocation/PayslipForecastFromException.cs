namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Functions of payslip forecast from exception.</summary>
public static class PayslipForecastFromException
{
    public static Dictionary<string, double> SummarizeBatchedGarnishmentExceptionsForForecast(List<BatchedGarnishmentException> items)
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

    public static List<PayslipForecast> DerivePayslipForecastsFromException(List<BatchedGarnishmentException> items)
    {
        var outValue = new List<PayslipForecast>();
        foreach (var item in items)
        {
            var derived = PayslipForecast.NewPayslipForecast(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputePayslipForecast(PayslipForecast item)
    {
        return FallbackGarnishmentExceptionPolicy.ClassifyFallbackGarnishmentException(item.NetTotal());
    }
}
