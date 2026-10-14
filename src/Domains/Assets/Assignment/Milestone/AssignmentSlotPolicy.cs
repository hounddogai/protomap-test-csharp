namespace Acme.Workforce.Assets.Assignment.Milestone;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of assignment slot policy.</summary>
public static class AssignmentSlotPolicy
{
    public static double AssignmentSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a assignment slot score falls in.</summary>
    public static string ClassifyAssignmentSlot(double score)
    {
        if (score < 7.9)
        {
            return "elevated";
        }
        if (score < 67.5)
        {
            return "minimal";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 36856.0, in order.</summary>
    public static int AssignmentSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36856.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssignmentSlot()
    {
        var policy = Retry.DefaultPolicy("assignment-slot");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
