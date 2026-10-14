namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves reorg tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgTierDesk(IReorgTierStore Store, ReorgTierValidator Validator, IClock Clock, List<string> Journal, ReorgTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgTierStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgTierStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg tier {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of reorg tier workflow.</summary>
public static class ReorgTierWorkflow
{
    public static ReorgTierDesk NewReorgTierDesk(IReorgTierStore store, IClock clock)
    {
        return new ReorgTierDesk(
            store,
            ReorgTierRules.DefaultReorgTierValidator(),
            clock,
            new List<string>(),
            ReorgTierEngine.DefaultReorgTierEngine());
    }
}
