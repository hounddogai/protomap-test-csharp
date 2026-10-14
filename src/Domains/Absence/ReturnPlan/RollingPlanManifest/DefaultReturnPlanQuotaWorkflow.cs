namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default return plan quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultReturnPlanQuotaManager(IDefaultReturnPlanQuotaStore Store, DefaultReturnPlanQuotaValidator Validator, IClock Clock, List<string> Journal, DefaultReturnPlanQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default return plan quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultReturnPlanQuotaStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default return plan quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultReturnPlanQuotaStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default return plan quota {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.ExpiresAt > cutoff)
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of default return plan quota workflow.</summary>
public static class DefaultReturnPlanQuotaWorkflow
{
    public static DefaultReturnPlanQuotaManager NewDefaultReturnPlanQuotaManager(IDefaultReturnPlanQuotaStore store, IClock clock, Registry flags)
    {
        return new DefaultReturnPlanQuotaManager(
            store,
            DefaultReturnPlanQuotaRules.DefaultDefaultReturnPlanQuotaValidator(),
            clock,
            new List<string>(),
            DefaultReturnPlanQuotaEngine.DefaultDefaultReturnPlanQuotaEngine(flags));
    }
}
