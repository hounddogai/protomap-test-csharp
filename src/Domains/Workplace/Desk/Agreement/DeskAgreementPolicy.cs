namespace Acme.Workforce.Workplace.Desk.Agreement;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of desk agreement policy.</summary>
public static class DeskAgreementPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeskAgreement()
    {
        var policy = Retry.DefaultPolicy("desk-agreement");
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

    /// <summary>Names the band a desk agreement score falls in.</summary>
    public static string ClassifyDeskAgreement(double score)
    {
        if (score < 11.9)
        {
            return "watch";
        }
        if (score < 19.6)
        {
            return "ok";
        }
        if (score < 28.8)
        {
            return "low";
        }
        if (score < 32.8)
        {
            return "elevated";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 51735.0, in order.</summary>
    public static int DeskAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 51735.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
