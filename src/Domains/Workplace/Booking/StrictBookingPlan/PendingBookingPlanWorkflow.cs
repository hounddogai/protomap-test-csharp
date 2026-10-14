namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending booking plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingBookingPlanWorkflow(IPendingBookingPlanStore Store, PendingBookingPlanValidator Validator, IClock Clock, List<string> Journal, PendingBookingPlanEngine Engine)
{
    public static PendingBookingPlanWorkflow NewPendingBookingPlanWorkflow(IPendingBookingPlanStore store, IClock clock, Registry flags)
    {
        return new PendingBookingPlanWorkflow(
            store,
            PendingBookingPlanRules.DefaultPendingBookingPlanValidator(),
            clock,
            new List<string>(),
            PendingBookingPlanEngine.DefaultPendingBookingPlanEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending booking plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingBookingPlanStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending booking plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingBookingPlanStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending booking plan {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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
