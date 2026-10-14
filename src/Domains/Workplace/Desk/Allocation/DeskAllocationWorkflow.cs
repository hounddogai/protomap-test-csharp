namespace Acme.Workforce.Workplace.Desk.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves desk allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskAllocationManager(IDeskAllocationStore Store, DeskAllocationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskAllocationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskAllocationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of desk allocation workflow.</summary>
public static class DeskAllocationWorkflow
{
    public static DeskAllocationManager NewDeskAllocationManager(IDeskAllocationStore store, IClock clock)
    {
        return new DeskAllocationManager(
            store,
            DeskAllocationRules.DefaultDeskAllocationValidator(),
            clock,
            new List<string>());
    }
}
