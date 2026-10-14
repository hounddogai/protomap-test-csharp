namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves accrual manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualManifestCoordinator(IAccrualManifestStore Store, AccrualManifestValidator Validator, IClock Clock, List<string> Journal, AccrualManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualManifestStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualManifestStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual manifest {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of accrual manifest workflow.</summary>
public static class AccrualManifestWorkflow
{
    public static AccrualManifestCoordinator NewAccrualManifestCoordinator(IAccrualManifestStore store, IClock clock, Registry flags)
    {
        return new AccrualManifestCoordinator(
            store,
            AccrualManifestRules.DefaultAccrualManifestValidator(),
            clock,
            new List<string>(),
            AccrualManifestEngine.DefaultAccrualManifestEngine(flags));
    }
}
