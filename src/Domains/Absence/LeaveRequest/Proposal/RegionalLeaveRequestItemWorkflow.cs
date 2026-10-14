namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional leave request items through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalLeaveRequestItemService(IRegionalLeaveRequestItemStore Store, RegionalLeaveRequestItemValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional leave request item {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalLeaveRequestItemStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional leave request item {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalLeaveRequestItemStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of regional leave request item workflow.</summary>
public static class RegionalLeaveRequestItemWorkflow
{
    public static RegionalLeaveRequestItemService NewRegionalLeaveRequestItemService(IRegionalLeaveRequestItemStore store, IClock clock)
    {
        return new RegionalLeaveRequestItemService(
            store,
            RegionalLeaveRequestItemRules.DefaultRegionalLeaveRequestItemValidator(),
            clock,
            new List<string>());
    }
}
