namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves exception checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionCheckpointCoordinator(IExceptionCheckpointStore Store, ExceptionCheckpointValidator Validator, IClock Clock, List<string> Journal, ExceptionCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionCheckpointStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionCheckpointStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception checkpoint {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of exception checkpoint workflow.</summary>
public static class ExceptionCheckpointWorkflow
{
    public static ExceptionCheckpointCoordinator NewExceptionCheckpointCoordinator(IExceptionCheckpointStore store, IClock clock, Registry flags)
    {
        return new ExceptionCheckpointCoordinator(
            store,
            ExceptionCheckpointRules.DefaultExceptionCheckpointValidator(),
            clock,
            new List<string>(),
            ExceptionCheckpointEngine.DefaultExceptionCheckpointEngine(flags));
    }
}
