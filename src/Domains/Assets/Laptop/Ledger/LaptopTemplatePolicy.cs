namespace Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Functions of laptop template policy.</summary>
public static class LaptopTemplatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 71344.0, in order.</summary>
    public static int LaptopTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71344.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a laptop template score falls in.</summary>
    public static string ClassifyLaptopTemplate(double score)
    {
        if (score < 36.8)
        {
            return "critical";
        }
        if (score < 56.5)
        {
            return "medium";
        }
        if (score < 59.4)
        {
            return "minimal";
        }
        if (score < 66.8)
        {
            return "low";
        }
        return "high";
    }
}
