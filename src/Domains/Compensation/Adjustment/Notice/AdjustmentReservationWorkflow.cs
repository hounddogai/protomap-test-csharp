namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentReservationService(IAdjustmentReservationStore Store, AdjustmentReservationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentReservationStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentReservationStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of adjustment reservation workflow.</summary>
public static class AdjustmentReservationWorkflow
{
    public static AdjustmentReservationService NewAdjustmentReservationService(IAdjustmentReservationStore store, IClock clock)
    {
        return new AdjustmentReservationService(
            store,
            AdjustmentReservationRules.DefaultAdjustmentReservationValidator(),
            clock,
            new List<string>());
    }
}
