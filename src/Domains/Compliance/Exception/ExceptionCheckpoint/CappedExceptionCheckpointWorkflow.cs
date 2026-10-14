namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped exception checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedExceptionCheckpointService(ICappedExceptionCheckpointStore Store, CappedExceptionCheckpointValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped exception checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedExceptionCheckpointStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped exception checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedExceptionCheckpointStatus.Expired);
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

/// <summary>Functions of capped exception checkpoint workflow.</summary>
public static class CappedExceptionCheckpointWorkflow
{
    public static CappedExceptionCheckpointService NewCappedExceptionCheckpointService(ICappedExceptionCheckpointStore store, IClock clock)
    {
        return new CappedExceptionCheckpointService(
            store,
            CappedExceptionCheckpointRules.DefaultCappedExceptionCheckpointValidator(),
            clock,
            new List<string>());
    }
}
