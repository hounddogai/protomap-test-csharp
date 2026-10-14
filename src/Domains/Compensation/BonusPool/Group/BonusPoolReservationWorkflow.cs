namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves bonus pool reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolReservationService(IBonusPoolReservationStore Store, BonusPoolReservationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolReservationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolReservationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of bonus pool reservation workflow.</summary>
public static class BonusPoolReservationWorkflow
{
    public static BonusPoolReservationService NewBonusPoolReservationService(IBonusPoolReservationStore store, IClock clock)
    {
        return new BonusPoolReservationService(
            store,
            BonusPoolReservationRules.DefaultBonusPoolReservationValidator(),
            clock,
            new List<string>());
    }
}
