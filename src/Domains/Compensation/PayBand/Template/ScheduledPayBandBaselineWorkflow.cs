namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled pay band baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledPayBandBaselineCoordinator(IScheduledPayBandBaselineStore Store, ScheduledPayBandBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay band baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPayBandBaselineStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay band baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPayBandBaselineStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of scheduled pay band baseline workflow.</summary>
public static class ScheduledPayBandBaselineWorkflow
{
    public static ScheduledPayBandBaselineCoordinator NewScheduledPayBandBaselineCoordinator(IScheduledPayBandBaselineStore store, IClock clock)
    {
        return new ScheduledPayBandBaselineCoordinator(
            store,
            ScheduledPayBandBaselineRules.DefaultScheduledPayBandBaselineValidator(),
            clock,
            new List<string>());
    }
}
