namespace Acme.Workforce.Workplace.Booking.BookingPlan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient booking plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientBookingPlanService(ILenientBookingPlanStore Store, LenientBookingPlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient booking plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientBookingPlanStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient booking plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientBookingPlanStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of lenient booking plan workflow.</summary>
public static class LenientBookingPlanWorkflow
{
    public static LenientBookingPlanService NewLenientBookingPlanService(ILenientBookingPlanStore store, IClock clock)
    {
        return new LenientBookingPlanService(
            store,
            LenientBookingPlanRules.DefaultLenientBookingPlanValidator(),
            clock,
            new List<string>());
    }
}
