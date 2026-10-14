namespace Acme.Workforce.Absence.Balance.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceCheckpointManager(IBalanceCheckpointStore Store, BalanceCheckpointValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceCheckpointStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceCheckpointStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of balance checkpoint workflow.</summary>
public static class BalanceCheckpointWorkflow
{
    public static BalanceCheckpointManager NewBalanceCheckpointManager(IBalanceCheckpointStore store, IClock clock)
    {
        return new BalanceCheckpointManager(
            store,
            BalanceCheckpointRules.DefaultBalanceCheckpointValidator(),
            clock,
            new List<string>());
    }
}
