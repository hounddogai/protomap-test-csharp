namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient desk allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientDeskAllocationManager(ILenientDeskAllocationStore Store, LenientDeskAllocationValidator Validator, IClock Clock, List<string> Journal, LenientDeskAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient desk allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientDeskAllocationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient desk allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientDeskAllocationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient desk allocation {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of lenient desk allocation workflow.</summary>
public static class LenientDeskAllocationWorkflow
{
    public static LenientDeskAllocationManager NewLenientDeskAllocationManager(ILenientDeskAllocationStore store, IClock clock, Registry flags)
    {
        return new LenientDeskAllocationManager(
            store,
            LenientDeskAllocationRules.DefaultLenientDeskAllocationValidator(),
            clock,
            new List<string>(),
            LenientDeskAllocationEngine.DefaultLenientDeskAllocationEngine(flags));
    }
}
