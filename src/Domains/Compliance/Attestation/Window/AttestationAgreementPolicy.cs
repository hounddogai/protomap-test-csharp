namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation agreement policy.</summary>
public static class AttestationAgreementPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 91837.0, in order.</summary>
    public static int AttestationAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91837.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double AttestationAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
