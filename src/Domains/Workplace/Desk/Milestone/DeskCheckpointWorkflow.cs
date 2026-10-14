namespace Acme.Workforce.Workplace.Desk.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves desk checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskCheckpointDesk(IDeskCheckpointStore Store, DeskCheckpointValidator Validator, IClock Clock, List<string> Journal, DeskCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskCheckpointStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskCheckpointStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk checkpoint {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of desk checkpoint workflow.</summary>
public static class DeskCheckpointWorkflow
{
    public static DeskCheckpointDesk NewDeskCheckpointDesk(IDeskCheckpointStore store, IClock clock, Registry flags)
    {
        return new DeskCheckpointDesk(
            store,
            DeskCheckpointRules.DefaultDeskCheckpointValidator(),
            clock,
            new List<string>(),
            DeskCheckpointEngine.DefaultDeskCheckpointEngine(flags));
    }
}
