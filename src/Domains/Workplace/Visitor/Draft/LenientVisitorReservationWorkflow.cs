namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient visitor reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientVisitorReservationDesk(ILenientVisitorReservationStore Store, LenientVisitorReservationValidator Validator, IClock Clock, List<string> Journal, LenientVisitorReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient visitor reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientVisitorReservationStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient visitor reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientVisitorReservationStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient visitor reservation {code} not found");
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

/// <summary>Functions of lenient visitor reservation workflow.</summary>
public static class LenientVisitorReservationWorkflow
{
    public static LenientVisitorReservationDesk NewLenientVisitorReservationDesk(ILenientVisitorReservationStore store, IClock clock, Registry flags)
    {
        return new LenientVisitorReservationDesk(
            store,
            LenientVisitorReservationRules.DefaultLenientVisitorReservationValidator(),
            clock,
            new List<string>(),
            LenientVisitorReservationEngine.DefaultLenientVisitorReservationEngine(flags));
    }
}
