namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled pool thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledPoolThresholdManager(IScheduledPoolThresholdStore Store, ScheduledPoolThresholdValidator Validator, IClock Clock, List<string> Journal, ScheduledPoolThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pool threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPoolThresholdStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pool threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPoolThresholdStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pool threshold {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of scheduled pool threshold workflow.</summary>
public static class ScheduledPoolThresholdWorkflow
{
    public static ScheduledPoolThresholdManager NewScheduledPoolThresholdManager(IScheduledPoolThresholdStore store, IClock clock)
    {
        return new ScheduledPoolThresholdManager(
            store,
            ScheduledPoolThresholdRules.DefaultScheduledPoolThresholdValidator(),
            clock,
            new List<string>(),
            ScheduledPoolThresholdEngine.DefaultScheduledPoolThresholdEngine());
    }
}
