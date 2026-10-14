namespace Acme.Workforce.Absence.ReturnPlan.Audit;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of return plan audit policy.</summary>
public static class ReturnPlanAuditPolicy
{
    public static double ReturnPlanAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a return plan audit score falls in.</summary>
    public static string ClassifyReturnPlanAudit(double score)
    {
        if (score < 5.0)
        {
            return "low";
        }
        if (score < 21.2)
        {
            return "watch";
        }
        if (score < 21.6)
        {
            return "elevated";
        }
        if (score < 35.9)
        {
            return "ok";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeReturnPlanAudit()
    {
        var policy = Retry.DefaultPolicy("return-plan-audit");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
