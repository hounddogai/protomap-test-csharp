namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled booking claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledBookingClaimService(IScheduledBookingClaimStore Store, ScheduledBookingClaimValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled booking claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledBookingClaimStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled booking claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledBookingClaimStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of scheduled booking claim workflow.</summary>
public static class ScheduledBookingClaimWorkflow
{
    public static ScheduledBookingClaimService NewScheduledBookingClaimService(IScheduledBookingClaimStore store, IClock clock)
    {
        return new ScheduledBookingClaimService(
            store,
            ScheduledBookingClaimRules.DefaultScheduledBookingClaimValidator(),
            clock,
            new List<string>());
    }
}
