namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global town hall checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalTownHallCheckpointCoordinator(IGlobalTownHallCheckpointStore Store, GlobalTownHallCheckpointValidator Validator, IClock Clock, List<string> Journal, GlobalTownHallCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global town hall checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalTownHallCheckpointStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global town hall checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalTownHallCheckpointStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global town hall checkpoint {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of global town hall checkpoint workflow.</summary>
public static class GlobalTownHallCheckpointWorkflow
{
    public static GlobalTownHallCheckpointCoordinator NewGlobalTownHallCheckpointCoordinator(IGlobalTownHallCheckpointStore store, IClock clock, Registry flags)
    {
        return new GlobalTownHallCheckpointCoordinator(
            store,
            GlobalTownHallCheckpointRules.DefaultGlobalTownHallCheckpointValidator(),
            clock,
            new List<string>(),
            GlobalTownHallCheckpointEngine.DefaultGlobalTownHallCheckpointEngine(flags));
    }
}
