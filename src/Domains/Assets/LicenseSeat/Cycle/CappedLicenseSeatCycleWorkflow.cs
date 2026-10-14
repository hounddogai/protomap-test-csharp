namespace Acme.Workforce.Assets.LicenseSeat.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped license seat cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedLicenseSeatCycleCoordinator(ICappedLicenseSeatCycleStore Store, CappedLicenseSeatCycleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped license seat cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedLicenseSeatCycleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped license seat cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedLicenseSeatCycleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of capped license seat cycle workflow.</summary>
public static class CappedLicenseSeatCycleWorkflow
{
    public static CappedLicenseSeatCycleCoordinator NewCappedLicenseSeatCycleCoordinator(ICappedLicenseSeatCycleStore store, IClock clock)
    {
        return new CappedLicenseSeatCycleCoordinator(
            store,
            CappedLicenseSeatCycleRules.DefaultCappedLicenseSeatCycleValidator(),
            clock,
            new List<string>());
    }
}
