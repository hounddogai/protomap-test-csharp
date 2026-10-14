namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary leave request budgets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryLeaveRequestBudgetService(IPrimaryLeaveRequestBudgetStore Store, PrimaryLeaveRequestBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary leave request budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryLeaveRequestBudgetStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary leave request budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryLeaveRequestBudgetStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of primary leave request budget workflow.</summary>
public static class PrimaryLeaveRequestBudgetWorkflow
{
    public static PrimaryLeaveRequestBudgetService NewPrimaryLeaveRequestBudgetService(IPrimaryLeaveRequestBudgetStore store, IClock clock)
    {
        return new PrimaryLeaveRequestBudgetService(
            store,
            PrimaryLeaveRequestBudgetRules.DefaultPrimaryLeaveRequestBudgetValidator(),
            clock,
            new List<string>());
    }
}
