namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleExceptionManager(IMeritCycleExceptionStore Store, MeritCycleExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleExceptionStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleExceptionStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of merit cycle exception workflow.</summary>
public static class MeritCycleExceptionWorkflow
{
    public static MeritCycleExceptionManager NewMeritCycleExceptionManager(IMeritCycleExceptionStore store, IClock clock)
    {
        return new MeritCycleExceptionManager(
            store,
            MeritCycleExceptionRules.DefaultMeritCycleExceptionValidator(),
            clock,
            new List<string>());
    }
}
