namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default bonus pool allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultBonusPoolAllocationDesk(IDefaultBonusPoolAllocationStore Store, DefaultBonusPoolAllocationValidator Validator, IClock Clock, List<string> Journal, DefaultBonusPoolAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default bonus pool allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultBonusPoolAllocationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default bonus pool allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultBonusPoolAllocationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default bonus pool allocation {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of default bonus pool allocation workflow.</summary>
public static class DefaultBonusPoolAllocationWorkflow
{
    public static DefaultBonusPoolAllocationDesk NewDefaultBonusPoolAllocationDesk(IDefaultBonusPoolAllocationStore store, IClock clock)
    {
        return new DefaultBonusPoolAllocationDesk(
            store,
            DefaultBonusPoolAllocationRules.DefaultDefaultBonusPoolAllocationValidator(),
            clock,
            new List<string>(),
            DefaultBonusPoolAllocationEngine.DefaultDefaultBonusPoolAllocationEngine());
    }
}
