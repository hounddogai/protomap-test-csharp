namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary attestation schedule policy.</summary>
public static class PrimaryAttestationSchedulePolicy
{
    public static int PrimaryAttestationScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 21921.0, in order.</summary>
    public static int PrimaryAttestationScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21921.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PrimaryAttestationScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
