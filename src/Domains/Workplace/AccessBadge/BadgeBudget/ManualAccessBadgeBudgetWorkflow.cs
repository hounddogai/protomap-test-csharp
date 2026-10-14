namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual access badge budgets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualAccessBadgeBudgetCoordinator(IManualAccessBadgeBudgetStore Store, ManualAccessBadgeBudgetValidator Validator, IClock Clock, List<string> Journal, ManualAccessBadgeBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual access badge budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualAccessBadgeBudgetStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual access badge budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualAccessBadgeBudgetStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual access badge budget {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of manual access badge budget workflow.</summary>
public static class ManualAccessBadgeBudgetWorkflow
{
    public static ManualAccessBadgeBudgetCoordinator NewManualAccessBadgeBudgetCoordinator(IManualAccessBadgeBudgetStore store, IClock clock, Registry flags)
    {
        return new ManualAccessBadgeBudgetCoordinator(
            store,
            ManualAccessBadgeBudgetRules.DefaultManualAccessBadgeBudgetValidator(),
            clock,
            new List<string>(),
            ManualAccessBadgeBudgetEngine.DefaultManualAccessBadgeBudgetEngine(flags));
    }
}
