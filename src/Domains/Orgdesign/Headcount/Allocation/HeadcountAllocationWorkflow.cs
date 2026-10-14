namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves headcount allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountAllocationManager(IHeadcountAllocationStore Store, HeadcountAllocationValidator Validator, IClock Clock, List<string> Journal, HeadcountAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountAllocationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountAllocationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount allocation {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of headcount allocation workflow.</summary>
public static class HeadcountAllocationWorkflow
{
    public static HeadcountAllocationManager NewHeadcountAllocationManager(IHeadcountAllocationStore store, IClock clock, Registry flags)
    {
        return new HeadcountAllocationManager(
            store,
            HeadcountAllocationRules.DefaultHeadcountAllocationValidator(),
            clock,
            new List<string>(),
            HeadcountAllocationEngine.DefaultHeadcountAllocationEngine(flags));
    }
}
