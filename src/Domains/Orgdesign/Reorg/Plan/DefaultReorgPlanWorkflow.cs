namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default reorg plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultReorgPlanService(IDefaultReorgPlanStore Store, DefaultReorgPlanValidator Validator, IClock Clock, List<string> Journal, DefaultReorgPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default reorg plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultReorgPlanStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default reorg plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultReorgPlanStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default reorg plan {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of default reorg plan workflow.</summary>
public static class DefaultReorgPlanWorkflow
{
    public static DefaultReorgPlanService NewDefaultReorgPlanService(IDefaultReorgPlanStore store, IClock clock, Registry flags)
    {
        return new DefaultReorgPlanService(
            store,
            DefaultReorgPlanRules.DefaultDefaultReorgPlanValidator(),
            clock,
            new List<string>(),
            DefaultReorgPlanEngine.DefaultDefaultReorgPlanEngine(flags));
    }
}
