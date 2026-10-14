namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional engagement budgets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalEngagementBudgetService(IRegionalEngagementBudgetStore Store, RegionalEngagementBudgetValidator Validator, IClock Clock, List<string> Journal, RegionalEngagementBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional engagement budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalEngagementBudgetStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional engagement budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalEngagementBudgetStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional engagement budget {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of regional engagement budget workflow.</summary>
public static class RegionalEngagementBudgetWorkflow
{
    public static RegionalEngagementBudgetService NewRegionalEngagementBudgetService(IRegionalEngagementBudgetStore store, IClock clock, Registry flags)
    {
        return new RegionalEngagementBudgetService(
            store,
            RegionalEngagementBudgetRules.DefaultRegionalEngagementBudgetValidator(),
            clock,
            new List<string>(),
            RegionalEngagementBudgetEngine.DefaultRegionalEngagementBudgetEngine(flags));
    }
}
