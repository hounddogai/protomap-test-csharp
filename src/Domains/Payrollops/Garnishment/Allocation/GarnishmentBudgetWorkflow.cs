namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves garnishment budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentBudgetManager(IGarnishmentBudgetStore Store, GarnishmentBudgetValidator Validator, IClock Clock, List<string> Journal, GarnishmentBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentBudgetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentBudgetStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment budget {code} not found");
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

/// <summary>Functions of garnishment budget workflow.</summary>
public static class GarnishmentBudgetWorkflow
{
    public static GarnishmentBudgetManager NewGarnishmentBudgetManager(IGarnishmentBudgetStore store, IClock clock)
    {
        return new GarnishmentBudgetManager(
            store,
            GarnishmentBudgetRules.DefaultGarnishmentBudgetValidator(),
            clock,
            new List<string>(),
            GarnishmentBudgetEngine.DefaultGarnishmentBudgetEngine());
    }
}
