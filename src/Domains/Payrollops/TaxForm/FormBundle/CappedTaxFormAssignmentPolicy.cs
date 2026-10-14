namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped tax form assignment policy.</summary>
public static class CappedTaxFormAssignmentPolicy
{
    public static string CappedTaxFormAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int CappedTaxFormAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a capped tax form assignment score falls in.</summary>
    public static string ClassifyCappedTaxFormAssignment(double score)
    {
        if (score < 9.0)
        {
            return "high";
        }
        if (score < 69.4)
        {
            return "elevated";
        }
        return "minimal";
    }
}
