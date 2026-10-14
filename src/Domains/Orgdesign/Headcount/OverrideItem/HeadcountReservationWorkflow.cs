namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves headcount reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountReservationWorkflow(IHeadcountReservationStore Store, HeadcountReservationValidator Validator, IClock Clock, List<string> Journal)
{
    public static HeadcountReservationWorkflow NewHeadcountReservationWorkflow(IHeadcountReservationStore store, IClock clock)
    {
        return new HeadcountReservationWorkflow(
            store,
            HeadcountReservationRules.DefaultHeadcountReservationValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountReservationStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountReservationStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
