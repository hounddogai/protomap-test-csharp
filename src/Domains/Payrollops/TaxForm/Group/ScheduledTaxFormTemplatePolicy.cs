namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled tax form template policy.</summary>
public static class ScheduledTaxFormTemplatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 10503.0, in order.</summary>
    public static int ScheduledTaxFormTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10503.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int ScheduledTaxFormTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a scheduled tax form template score falls in.</summary>
    public static string ClassifyScheduledTaxFormTemplate(double score)
    {
        if (score < 64.7)
        {
            return "low";
        }
        if (score < 67.7)
        {
            return "ok";
        }
        return "watch";
    }

    public static string ScheduledTaxFormTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
