namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled town hall exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledTownHallExceptionService(IScheduledTownHallExceptionStore Store, ScheduledTownHallExceptionValidator Validator, IClock Clock, List<string> Journal, ScheduledTownHallExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled town hall exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledTownHallExceptionStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled town hall exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledTownHallExceptionStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled town hall exception {code} not found");
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

/// <summary>Functions of scheduled town hall exception workflow.</summary>
public static class ScheduledTownHallExceptionWorkflow
{
    public static ScheduledTownHallExceptionService NewScheduledTownHallExceptionService(IScheduledTownHallExceptionStore store, IClock clock)
    {
        return new ScheduledTownHallExceptionService(
            store,
            ScheduledTownHallExceptionRules.DefaultScheduledTownHallExceptionValidator(),
            clock,
            new List<string>(),
            ScheduledTownHallExceptionEngine.DefaultScheduledTownHallExceptionEngine());
    }
}
