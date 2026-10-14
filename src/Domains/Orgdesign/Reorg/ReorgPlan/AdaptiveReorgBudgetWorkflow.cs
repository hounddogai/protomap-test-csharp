namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive reorg budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveReorgBudgetDesk(IAdaptiveReorgBudgetStore Store, AdaptiveReorgBudgetValidator Validator, IClock Clock, List<string> Journal, AdaptiveReorgBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive reorg budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveReorgBudgetStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive reorg budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveReorgBudgetStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive reorg budget {code} not found");
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

/// <summary>Functions of adaptive reorg budget workflow.</summary>
public static class AdaptiveReorgBudgetWorkflow
{
    public static AdaptiveReorgBudgetDesk NewAdaptiveReorgBudgetDesk(IAdaptiveReorgBudgetStore store, IClock clock, Registry flags)
    {
        return new AdaptiveReorgBudgetDesk(
            store,
            AdaptiveReorgBudgetRules.DefaultAdaptiveReorgBudgetValidator(),
            clock,
            new List<string>(),
            AdaptiveReorgBudgetEngine.DefaultAdaptiveReorgBudgetEngine(flags));
    }
}
