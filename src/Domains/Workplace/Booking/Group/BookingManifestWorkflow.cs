namespace Acme.Workforce.Workplace.Booking.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves booking manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record BookingManifestService(IBookingManifestStore Store, BookingManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BookingManifestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BookingManifestStatus.Failed);
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

/// <summary>Functions of booking manifest workflow.</summary>
public static class BookingManifestWorkflow
{
    public static BookingManifestService NewBookingManifestService(IBookingManifestStore store, IClock clock)
    {
        return new BookingManifestService(
            store,
            BookingManifestRules.DefaultBookingManifestValidator(),
            clock,
            new List<string>());
    }
}
