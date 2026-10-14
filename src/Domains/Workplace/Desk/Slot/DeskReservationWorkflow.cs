namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves desk reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskReservationWorkflow(IDeskReservationStore Store, DeskReservationValidator Validator, IClock Clock, List<string> Journal)
{
    public static DeskReservationWorkflow NewDeskReservationWorkflow(IDeskReservationStore store, IClock clock)
    {
        return new DeskReservationWorkflow(
            store,
            DeskReservationRules.DefaultDeskReservationValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskReservationStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskReservationStatus.Withdrawn);
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
