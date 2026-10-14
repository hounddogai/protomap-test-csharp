namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualTargetDesk(IAccrualTargetStore Store, AccrualTargetValidator Validator, IClock Clock, List<string> Journal, AccrualTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualTargetStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualTargetStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual target {code} not found");
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

/// <summary>Functions of accrual target workflow.</summary>
public static class AccrualTargetWorkflow
{
    public static AccrualTargetDesk NewAccrualTargetDesk(IAccrualTargetStore store, IClock clock)
    {
        return new AccrualTargetDesk(
            store,
            AccrualTargetRules.DefaultAccrualTargetValidator(),
            clock,
            new List<string>(),
            AccrualTargetEngine.DefaultAccrualTargetEngine());
    }
}
