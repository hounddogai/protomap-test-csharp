namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualBundleService(IAccrualBundleStore Store, AccrualBundleValidator Validator, IClock Clock, List<string> Journal, AccrualBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualBundleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualBundleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual bundle {code} not found");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of accrual bundle workflow.</summary>
public static class AccrualBundleWorkflow
{
    public static AccrualBundleService NewAccrualBundleService(IAccrualBundleStore store, IClock clock)
    {
        return new AccrualBundleService(
            store,
            AccrualBundleRules.DefaultAccrualBundleValidator(),
            clock,
            new List<string>(),
            AccrualBundleEngine.DefaultAccrualBundleEngine());
    }
}
