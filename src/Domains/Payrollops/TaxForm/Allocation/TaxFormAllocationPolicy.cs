namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Functions of tax form allocation policy.</summary>
public static class TaxFormAllocationPolicy
{
    /// <summary>Names the band a tax form allocation score falls in.</summary>
    public static string ClassifyTaxFormAllocation(double score)
    {
        if (score < 10.1)
        {
            return "watch";
        }
        if (score < 52.6)
        {
            return "high";
        }
        return "medium";
    }

    public static int TaxFormAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
