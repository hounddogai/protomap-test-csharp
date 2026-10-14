namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual accrual checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualAccrualCheckpointManager(IManualAccrualCheckpointStore Store, ManualAccrualCheckpointValidator Validator, IClock Clock, List<string> Journal, ManualAccrualCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual accrual checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualAccrualCheckpointStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual accrual checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualAccrualCheckpointStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual accrual checkpoint {code} not found");
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

/// <summary>Functions of manual accrual checkpoint workflow.</summary>
public static class ManualAccrualCheckpointWorkflow
{
    public static ManualAccrualCheckpointManager NewManualAccrualCheckpointManager(IManualAccrualCheckpointStore store, IClock clock, Registry flags)
    {
        return new ManualAccrualCheckpointManager(
            store,
            ManualAccrualCheckpointRules.DefaultManualAccrualCheckpointValidator(),
            clock,
            new List<string>(),
            ManualAccrualCheckpointEngine.DefaultManualAccrualCheckpointEngine(flags));
    }
}
