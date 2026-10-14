namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Functions of primary scenario segment policy.</summary>
public static class PrimaryScenarioSegmentPolicy
{
    public static int PrimaryScenarioSegmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 15420.0, in order.</summary>
    public static int PrimaryScenarioSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15420.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
