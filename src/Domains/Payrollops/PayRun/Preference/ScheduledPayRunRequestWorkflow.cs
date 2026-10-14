namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled pay run requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledPayRunRequestCoordinator(IScheduledPayRunRequestStore Store, ScheduledPayRunRequestValidator Validator, IClock Clock, List<string> Journal, ScheduledPayRunRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay run request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPayRunRequestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay run request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPayRunRequestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay run request {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of scheduled pay run request workflow.</summary>
public static class ScheduledPayRunRequestWorkflow
{
    public static ScheduledPayRunRequestCoordinator NewScheduledPayRunRequestCoordinator(IScheduledPayRunRequestStore store, IClock clock)
    {
        return new ScheduledPayRunRequestCoordinator(
            store,
            ScheduledPayRunRequestRules.DefaultScheduledPayRunRequestValidator(),
            clock,
            new List<string>(),
            ScheduledPayRunRequestEngine.DefaultScheduledPayRunRequestEngine());
    }
}
