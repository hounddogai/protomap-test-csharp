namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallRequestDesk(ITownHallRequestStore Store, TownHallRequestValidator Validator, IClock Clock, List<string> Journal, TownHallRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallRequestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallRequestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall request {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of town hall request workflow.</summary>
public static class TownHallRequestWorkflow
{
    public static TownHallRequestDesk NewTownHallRequestDesk(ITownHallRequestStore store, IClock clock)
    {
        return new TownHallRequestDesk(
            store,
            TownHallRequestRules.DefaultTownHallRequestValidator(),
            clock,
            new List<string>(),
            TownHallRequestEngine.DefaultTownHallRequestEngine());
    }
}
