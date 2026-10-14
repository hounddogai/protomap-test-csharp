namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled booking baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledBookingBaselineWorkflow(IScheduledBookingBaselineStore Store, ScheduledBookingBaselineValidator Validator, IClock Clock, List<string> Journal, ScheduledBookingBaselineEngine Engine)
{
    public static ScheduledBookingBaselineWorkflow NewScheduledBookingBaselineWorkflow(IScheduledBookingBaselineStore store, IClock clock)
    {
        return new ScheduledBookingBaselineWorkflow(
            store,
            ScheduledBookingBaselineRules.DefaultScheduledBookingBaselineValidator(),
            clock,
            new List<string>(),
            ScheduledBookingBaselineEngine.DefaultScheduledBookingBaselineEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled booking baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledBookingBaselineStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled booking baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledBookingBaselineStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled booking baseline {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
