namespace Acme.Workforce.Absence.Accrual.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualEstimateCoordinator(IAccrualEstimateStore Store, AccrualEstimateValidator Validator, IClock Clock, List<string> Journal, AccrualEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualEstimateStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualEstimateStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual estimate {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of accrual estimate workflow.</summary>
public static class AccrualEstimateWorkflow
{
    public static AccrualEstimateCoordinator NewAccrualEstimateCoordinator(IAccrualEstimateStore store, IClock clock)
    {
        return new AccrualEstimateCoordinator(
            store,
            AccrualEstimateRules.DefaultAccrualEstimateValidator(),
            clock,
            new List<string>(),
            AccrualEstimateEngine.DefaultAccrualEstimateEngine());
    }
}
