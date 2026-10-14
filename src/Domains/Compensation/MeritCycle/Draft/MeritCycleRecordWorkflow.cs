namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle records through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleRecordDesk(IMeritCycleRecordStore Store, MeritCycleRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleRecordStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleRecordStatus.Closed);
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

/// <summary>Functions of merit cycle record workflow.</summary>
public static class MeritCycleRecordWorkflow
{
    public static MeritCycleRecordDesk NewMeritCycleRecordDesk(IMeritCycleRecordStore store, IClock clock)
    {
        return new MeritCycleRecordDesk(
            store,
            MeritCycleRecordRules.DefaultMeritCycleRecordValidator(),
            clock,
            new List<string>());
    }
}
