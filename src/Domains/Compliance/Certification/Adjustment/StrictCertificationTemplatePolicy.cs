namespace Acme.Workforce.Compliance.Certification.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict certification template policy.</summary>
public static class StrictCertificationTemplatePolicy
{
    public static double StrictCertificationTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 16900.0, in order.</summary>
    public static int StrictCertificationTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 16900.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
