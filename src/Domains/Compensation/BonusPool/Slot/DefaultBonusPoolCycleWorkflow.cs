namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default bonus pool cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultBonusPoolCycleCoordinator(IDefaultBonusPoolCycleStore Store, DefaultBonusPoolCycleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default bonus pool cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultBonusPoolCycleStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default bonus pool cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultBonusPoolCycleStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
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

/// <summary>Functions of default bonus pool cycle workflow.</summary>
public static class DefaultBonusPoolCycleWorkflow
{
    public static DefaultBonusPoolCycleCoordinator NewDefaultBonusPoolCycleCoordinator(IDefaultBonusPoolCycleStore store, IClock clock)
    {
        return new DefaultBonusPoolCycleCoordinator(
            store,
            DefaultBonusPoolCycleRules.DefaultDefaultBonusPoolCycleValidator(),
            clock,
            new List<string>());
    }
}
