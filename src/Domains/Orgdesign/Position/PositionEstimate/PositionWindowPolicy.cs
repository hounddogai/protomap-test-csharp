namespace Acme.Workforce.Orgdesign.Position.PositionEstimate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of position window policy.</summary>
public static class PositionWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 38844.0, in order.</summary>
    public static int PositionWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38844.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePositionWindow()
    {
        var policy = Retry.DefaultPolicy("position-window");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
