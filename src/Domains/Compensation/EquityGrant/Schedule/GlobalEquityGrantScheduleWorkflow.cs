namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global equity grant schedules through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalEquityGrantScheduleService(IGlobalEquityGrantScheduleStore Store, GlobalEquityGrantScheduleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global equity grant schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalEquityGrantScheduleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global equity grant schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalEquityGrantScheduleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of global equity grant schedule workflow.</summary>
public static class GlobalEquityGrantScheduleWorkflow
{
    public static GlobalEquityGrantScheduleService NewGlobalEquityGrantScheduleService(IGlobalEquityGrantScheduleStore store, IClock clock)
    {
        return new GlobalEquityGrantScheduleService(
            store,
            GlobalEquityGrantScheduleRules.DefaultGlobalEquityGrantScheduleValidator(),
            clock,
            new List<string>());
    }
}
