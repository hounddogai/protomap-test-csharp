namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of desk template policy.</summary>
public static class DeskTemplatePolicy
{
    public static int DeskTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a desk template score falls in.</summary>
    public static string ClassifyDeskTemplate(double score)
    {
        if (score < 45.7)
        {
            return "elevated";
        }
        if (score < 89.0)
        {
            return "ok";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 79966.0, in order.</summary>
    public static int DeskTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79966.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double DeskTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
