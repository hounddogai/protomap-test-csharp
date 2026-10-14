namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy engagement budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyEngagementBudgetDesk(ILegacyEngagementBudgetStore Store, LegacyEngagementBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy engagement budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyEngagementBudgetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy engagement budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyEngagementBudgetStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of legacy engagement budget workflow.</summary>
public static class LegacyEngagementBudgetWorkflow
{
    public static LegacyEngagementBudgetDesk NewLegacyEngagementBudgetDesk(ILegacyEngagementBudgetStore store, IClock clock)
    {
        return new LegacyEngagementBudgetDesk(
            store,
            LegacyEngagementBudgetRules.DefaultLegacyEngagementBudgetValidator(),
            clock,
            new List<string>());
    }
}
