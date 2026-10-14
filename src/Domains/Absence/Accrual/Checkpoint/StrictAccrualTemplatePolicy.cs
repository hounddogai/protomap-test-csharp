namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict accrual template policy.</summary>
public static class StrictAccrualTemplatePolicy
{
    /// <summary>Names the band a strict accrual template score falls in.</summary>
    public static string ClassifyStrictAccrualTemplate(double score)
    {
        if (score < 48.0)
        {
            return "low";
        }
        if (score < 67.3)
        {
            return "elevated";
        }
        if (score < 93.0)
        {
            return "minimal";
        }
        return "ok";
    }

    public static double StrictAccrualTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
