namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tax form forecast policy.</summary>
public static class TaxFormForecastPolicy
{
    public static List<string> MergeTaxFormForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string TaxFormForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 6054.0, in order.</summary>
    public static int TaxFormForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 6054.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a tax form forecast score falls in.</summary>
    public static string ClassifyTaxFormForecast(double score)
    {
        if (score < 25.0)
        {
            return "high";
        }
        if (score < 80.1)
        {
            return "medium";
        }
        return "critical";
    }
}
