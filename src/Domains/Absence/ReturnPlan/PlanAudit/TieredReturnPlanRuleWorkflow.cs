namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered return plan rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredReturnPlanRuleCoordinator(ITieredReturnPlanRuleStore Store, TieredReturnPlanRuleValidator Validator, IClock Clock, List<string> Journal, TieredReturnPlanRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredReturnPlanRuleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredReturnPlanRuleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan rule {code} not found");
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

/// <summary>Functions of tiered return plan rule workflow.</summary>
public static class TieredReturnPlanRuleWorkflow
{
    public static TieredReturnPlanRuleCoordinator NewTieredReturnPlanRuleCoordinator(ITieredReturnPlanRuleStore store, IClock clock, Registry flags)
    {
        return new TieredReturnPlanRuleCoordinator(
            store,
            TieredReturnPlanRuleRules.DefaultTieredReturnPlanRuleValidator(),
            clock,
            new List<string>(),
            TieredReturnPlanRuleEngine.DefaultTieredReturnPlanRuleEngine(flags));
    }
}
