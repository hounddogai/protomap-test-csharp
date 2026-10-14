namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of training due group policy.</summary>
public static class TrainingDueGroupPolicy
{
    public static double TrainingDueGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.19;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a training due group score falls in.</summary>
    public static string ClassifyTrainingDueGroup(double score)
    {
        if (score < 20.2)
        {
            return "ok";
        }
        if (score < 21.7)
        {
            return "medium";
        }
        if (score < 81.4)
        {
            return "low";
        }
        return "critical";
    }
}
