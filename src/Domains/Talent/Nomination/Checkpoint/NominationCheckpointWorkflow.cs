namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves nomination checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationCheckpointManager(INominationCheckpointStore Store, NominationCheckpointValidator Validator, IClock Clock, List<string> Journal, NominationCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationCheckpointStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationCheckpointStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination checkpoint {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of nomination checkpoint workflow.</summary>
public static class NominationCheckpointWorkflow
{
    public static NominationCheckpointManager NewNominationCheckpointManager(INominationCheckpointStore store, IClock clock, Registry flags)
    {
        return new NominationCheckpointManager(
            store,
            NominationCheckpointRules.DefaultNominationCheckpointValidator(),
            clock,
            new List<string>(),
            NominationCheckpointEngine.DefaultNominationCheckpointEngine(flags));
    }
}
