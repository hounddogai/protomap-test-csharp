namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleWindowCoordinator(IMeritCycleWindowStore Store, MeritCycleWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleWindowStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleWindowStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of merit cycle window workflow.</summary>
public static class MeritCycleWindowWorkflow
{
    public static MeritCycleWindowCoordinator NewMeritCycleWindowCoordinator(IMeritCycleWindowStore store, IClock clock)
    {
        return new MeritCycleWindowCoordinator(
            store,
            MeritCycleWindowRules.DefaultMeritCycleWindowValidator(),
            clock,
            new List<string>());
    }
}
