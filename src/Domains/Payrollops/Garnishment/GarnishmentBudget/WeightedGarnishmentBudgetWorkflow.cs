namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted garnishment budgets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedGarnishmentBudgetService(IWeightedGarnishmentBudgetStore Store, WeightedGarnishmentBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted garnishment budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedGarnishmentBudgetStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted garnishment budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedGarnishmentBudgetStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of weighted garnishment budget workflow.</summary>
public static class WeightedGarnishmentBudgetWorkflow
{
    public static WeightedGarnishmentBudgetService NewWeightedGarnishmentBudgetService(IWeightedGarnishmentBudgetStore store, IClock clock)
    {
        return new WeightedGarnishmentBudgetService(
            store,
            WeightedGarnishmentBudgetRules.DefaultWeightedGarnishmentBudgetValidator(),
            clock,
            new List<string>());
    }
}
