namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional laptop budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalLaptopBudgetWorkflow(IRegionalLaptopBudgetStore Store, RegionalLaptopBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    public static RegionalLaptopBudgetWorkflow NewRegionalLaptopBudgetWorkflow(IRegionalLaptopBudgetStore store, IClock clock)
    {
        return new RegionalLaptopBudgetWorkflow(
            store,
            RegionalLaptopBudgetRules.DefaultRegionalLaptopBudgetValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional laptop budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalLaptopBudgetStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional laptop budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalLaptopBudgetStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
