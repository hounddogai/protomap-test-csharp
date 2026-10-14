namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict headcount reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictHeadcountReservationService(IStrictHeadcountReservationStore Store, StrictHeadcountReservationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict headcount reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictHeadcountReservationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict headcount reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictHeadcountReservationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of strict headcount reservation workflow.</summary>
public static class StrictHeadcountReservationWorkflow
{
    public static StrictHeadcountReservationService NewStrictHeadcountReservationService(IStrictHeadcountReservationStore store, IClock clock)
    {
        return new StrictHeadcountReservationService(
            store,
            StrictHeadcountReservationRules.DefaultStrictHeadcountReservationValidator(),
            clock,
            new List<string>());
    }
}
