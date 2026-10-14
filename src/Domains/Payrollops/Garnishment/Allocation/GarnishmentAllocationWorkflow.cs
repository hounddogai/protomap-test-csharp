namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves garnishment allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentAllocationDesk(IGarnishmentAllocationStore Store, GarnishmentAllocationValidator Validator, IClock Clock, List<string> Journal, GarnishmentAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentAllocationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentAllocationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment allocation {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of garnishment allocation workflow.</summary>
public static class GarnishmentAllocationWorkflow
{
    public static GarnishmentAllocationDesk NewGarnishmentAllocationDesk(IGarnishmentAllocationStore store, IClock clock, Registry flags)
    {
        return new GarnishmentAllocationDesk(
            store,
            GarnishmentAllocationRules.DefaultGarnishmentAllocationValidator(),
            clock,
            new List<string>(),
            GarnishmentAllocationEngine.DefaultGarnishmentAllocationEngine(flags));
    }
}
