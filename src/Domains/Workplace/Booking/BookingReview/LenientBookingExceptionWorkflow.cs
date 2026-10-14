namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient booking exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientBookingExceptionManager(ILenientBookingExceptionStore Store, LenientBookingExceptionValidator Validator, IClock Clock, List<string> Journal, LenientBookingExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient booking exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientBookingExceptionStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient booking exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientBookingExceptionStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient booking exception {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of lenient booking exception workflow.</summary>
public static class LenientBookingExceptionWorkflow
{
    public static LenientBookingExceptionManager NewLenientBookingExceptionManager(ILenientBookingExceptionStore store, IClock clock, Registry flags)
    {
        return new LenientBookingExceptionManager(
            store,
            LenientBookingExceptionRules.DefaultLenientBookingExceptionValidator(),
            clock,
            new List<string>(),
            LenientBookingExceptionEngine.DefaultLenientBookingExceptionEngine(flags));
    }
}
