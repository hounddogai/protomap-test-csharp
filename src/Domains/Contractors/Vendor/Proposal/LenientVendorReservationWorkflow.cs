namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient vendor reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientVendorReservationCoordinator(ILenientVendorReservationStore Store, LenientVendorReservationValidator Validator, IClock Clock, List<string> Journal, LenientVendorReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient vendor reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientVendorReservationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient vendor reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientVendorReservationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient vendor reservation {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of lenient vendor reservation workflow.</summary>
public static class LenientVendorReservationWorkflow
{
    public static LenientVendorReservationCoordinator NewLenientVendorReservationCoordinator(ILenientVendorReservationStore store, IClock clock)
    {
        return new LenientVendorReservationCoordinator(
            store,
            LenientVendorReservationRules.DefaultLenientVendorReservationValidator(),
            clock,
            new List<string>(),
            LenientVendorReservationEngine.DefaultLenientVendorReservationEngine());
    }
}
