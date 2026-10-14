namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves booking budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record BookingBudgetCoordinator(IBookingBudgetStore Store, BookingBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BookingBudgetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BookingBudgetStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of booking budget workflow.</summary>
public static class BookingBudgetWorkflow
{
    public static BookingBudgetCoordinator NewBookingBudgetCoordinator(IBookingBudgetStore store, IClock clock)
    {
        return new BookingBudgetCoordinator(
            store,
            BookingBudgetRules.DefaultBookingBudgetValidator(),
            clock,
            new List<string>());
    }
}
