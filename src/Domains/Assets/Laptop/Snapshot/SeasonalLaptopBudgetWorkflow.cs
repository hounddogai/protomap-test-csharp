namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal laptop budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalLaptopBudgetWorkflow(ISeasonalLaptopBudgetStore Store, SeasonalLaptopBudgetValidator Validator, IClock Clock, List<string> Journal, SeasonalLaptopBudgetEngine Engine)
{
    public static SeasonalLaptopBudgetWorkflow NewSeasonalLaptopBudgetWorkflow(ISeasonalLaptopBudgetStore store, IClock clock, Registry flags)
    {
        return new SeasonalLaptopBudgetWorkflow(
            store,
            SeasonalLaptopBudgetRules.DefaultSeasonalLaptopBudgetValidator(),
            clock,
            new List<string>(),
            SeasonalLaptopBudgetEngine.DefaultSeasonalLaptopBudgetEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal laptop budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalLaptopBudgetStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal laptop budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalLaptopBudgetStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal laptop budget {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.StartsAt > cutoff)
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
