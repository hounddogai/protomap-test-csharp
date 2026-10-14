namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy training due batch policy.</summary>
public static class LegacyTrainingDueBatchPolicy
{
    public static string LegacyTrainingDueBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a legacy training due batch score falls in.</summary>
    public static string ClassifyLegacyTrainingDueBatch(double score)
    {
        if (score < 10.5)
        {
            return "high";
        }
        if (score < 19.9)
        {
            return "low";
        }
        if (score < 56.2)
        {
            return "elevated";
        }
        if (score < 93.2)
        {
            return "ok";
        }
        return "minimal";
    }
}
