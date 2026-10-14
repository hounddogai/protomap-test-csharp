namespace Acme.Workforce.Compliance.Exception.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal exception checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalExceptionCheckpointCoordinator(ISeasonalExceptionCheckpointStore Store, SeasonalExceptionCheckpointValidator Validator, IClock Clock, List<string> Journal, SeasonalExceptionCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal exception checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalExceptionCheckpointStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal exception checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalExceptionCheckpointStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal exception checkpoint {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of seasonal exception checkpoint workflow.</summary>
public static class SeasonalExceptionCheckpointWorkflow
{
    public static SeasonalExceptionCheckpointCoordinator NewSeasonalExceptionCheckpointCoordinator(ISeasonalExceptionCheckpointStore store, IClock clock, Registry flags)
    {
        return new SeasonalExceptionCheckpointCoordinator(
            store,
            SeasonalExceptionCheckpointRules.DefaultSeasonalExceptionCheckpointValidator(),
            clock,
            new List<string>(),
            SeasonalExceptionCheckpointEngine.DefaultSeasonalExceptionCheckpointEngine(flags));
    }
}
