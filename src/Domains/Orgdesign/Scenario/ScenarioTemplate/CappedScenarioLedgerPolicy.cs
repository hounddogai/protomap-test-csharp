namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped scenario ledger policy.</summary>
public static class CappedScenarioLedgerPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedScenarioLedger()
    {
        var policy = Retry.DefaultPolicy("capped-scenario-ledger");
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

    public static double CappedScenarioLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a capped scenario ledger score falls in.</summary>
    public static string ClassifyCappedScenarioLedger(double score)
    {
        if (score < 1.5)
        {
            return "low";
        }
        if (score < 50.0)
        {
            return "ok";
        }
        if (score < 71.5)
        {
            return "watch";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 13509.0, in order.</summary>
    public static int CappedScenarioLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13509.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
