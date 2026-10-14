namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves accrual checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualCheckpointDesk(IAccrualCheckpointStore Store, AccrualCheckpointValidator Validator, IClock Clock, List<string> Journal, AccrualCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualCheckpointStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualCheckpointStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual checkpoint {code} not found");
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

/// <summary>Functions of accrual checkpoint workflow.</summary>
public static class AccrualCheckpointWorkflow
{
    public static AccrualCheckpointDesk NewAccrualCheckpointDesk(IAccrualCheckpointStore store, IClock clock, Registry flags)
    {
        return new AccrualCheckpointDesk(
            store,
            AccrualCheckpointRules.DefaultAccrualCheckpointValidator(),
            clock,
            new List<string>(),
            AccrualCheckpointEngine.DefaultAccrualCheckpointEngine(flags));
    }
}
