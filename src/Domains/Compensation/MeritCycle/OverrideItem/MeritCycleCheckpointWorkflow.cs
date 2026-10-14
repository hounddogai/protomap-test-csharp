namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleCheckpointCoordinator(IMeritCycleCheckpointStore Store, MeritCycleCheckpointValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleCheckpointStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleCheckpointStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of merit cycle checkpoint workflow.</summary>
public static class MeritCycleCheckpointWorkflow
{
    public static MeritCycleCheckpointCoordinator NewMeritCycleCheckpointCoordinator(IMeritCycleCheckpointStore store, IClock clock)
    {
        return new MeritCycleCheckpointCoordinator(
            store,
            MeritCycleCheckpointRules.DefaultMeritCycleCheckpointValidator(),
            clock,
            new List<string>());
    }
}
