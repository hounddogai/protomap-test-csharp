namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending pool slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingPoolSlotDesk(IPendingPoolSlotStore Store, PendingPoolSlotValidator Validator, IClock Clock, List<string> Journal, PendingPoolSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pool slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingPoolSlotStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pool slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingPoolSlotStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pool slot {code} not found");
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

/// <summary>Functions of pending pool slot workflow.</summary>
public static class PendingPoolSlotWorkflow
{
    public static PendingPoolSlotDesk NewPendingPoolSlotDesk(IPendingPoolSlotStore store, IClock clock)
    {
        return new PendingPoolSlotDesk(
            store,
            PendingPoolSlotRules.DefaultPendingPoolSlotValidator(),
            clock,
            new List<string>(),
            PendingPoolSlotEngine.DefaultPendingPoolSlotEngine());
    }
}
