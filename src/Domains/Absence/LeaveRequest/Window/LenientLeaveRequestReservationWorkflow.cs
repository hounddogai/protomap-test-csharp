namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient leave request reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientLeaveRequestReservationWorkflow(ILenientLeaveRequestReservationStore Store, LenientLeaveRequestReservationValidator Validator, IClock Clock, List<string> Journal, LenientLeaveRequestReservationEngine Engine)
{
    public static LenientLeaveRequestReservationWorkflow NewLenientLeaveRequestReservationWorkflow(ILenientLeaveRequestReservationStore store, IClock clock)
    {
        return new LenientLeaveRequestReservationWorkflow(
            store,
            LenientLeaveRequestReservationRules.DefaultLenientLeaveRequestReservationValidator(),
            clock,
            new List<string>(),
            LenientLeaveRequestReservationEngine.DefaultLenientLeaveRequestReservationEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient leave request reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientLeaveRequestReservationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient leave request reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientLeaveRequestReservationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient leave request reservation {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
