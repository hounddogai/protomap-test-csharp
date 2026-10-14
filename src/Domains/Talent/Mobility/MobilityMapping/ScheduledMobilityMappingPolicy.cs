namespace Acme.Workforce.Talent.Mobility.MobilityMapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled mobility mapping policy.</summary>
public static class ScheduledMobilityMappingPolicy
{
    public static string ScheduledMobilityMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 96589.0, in order.</summary>
    public static int ScheduledMobilityMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 96589.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
