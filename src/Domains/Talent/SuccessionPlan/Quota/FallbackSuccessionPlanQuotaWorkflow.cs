namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback succession plan quotas through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackSuccessionPlanQuotaManager(IFallbackSuccessionPlanQuotaStore Store, FallbackSuccessionPlanQuotaValidator Validator, IClock Clock, List<string> Journal, FallbackSuccessionPlanQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback succession plan quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackSuccessionPlanQuotaStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback succession plan quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackSuccessionPlanQuotaStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback succession plan quota {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of fallback succession plan quota workflow.</summary>
public static class FallbackSuccessionPlanQuotaWorkflow
{
    public static FallbackSuccessionPlanQuotaManager NewFallbackSuccessionPlanQuotaManager(IFallbackSuccessionPlanQuotaStore store, IClock clock, Registry flags)
    {
        return new FallbackSuccessionPlanQuotaManager(
            store,
            FallbackSuccessionPlanQuotaRules.DefaultFallbackSuccessionPlanQuotaValidator(),
            clock,
            new List<string>(),
            FallbackSuccessionPlanQuotaEngine.DefaultFallbackSuccessionPlanQuotaEngine(flags));
    }
}
