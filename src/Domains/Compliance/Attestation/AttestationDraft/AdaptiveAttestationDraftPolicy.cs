namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive attestation draft policy.</summary>
public static class AdaptiveAttestationDraftPolicy
{
    /// <summary>Names the band a adaptive attestation draft score falls in.</summary>
    public static string ClassifyAdaptiveAttestationDraft(double score)
    {
        if (score < 21.1)
        {
            return "low";
        }
        if (score < 75.9)
        {
            return "high";
        }
        if (score < 92.0)
        {
            return "medium";
        }
        return "ok";
    }

    public static string AdaptiveAttestationDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 29870.0, in order.</summary>
    public static int AdaptiveAttestationDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29870.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
