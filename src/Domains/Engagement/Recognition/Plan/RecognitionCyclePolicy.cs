namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition cycle policy.</summary>
public static class RecognitionCyclePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 4881.0, in order.</summary>
    public static int RecognitionCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4881.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RecognitionCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double RecognitionCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
