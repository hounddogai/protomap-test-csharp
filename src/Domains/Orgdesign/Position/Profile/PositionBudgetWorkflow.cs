namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionBudgetCoordinator(IPositionBudgetStore Store, PositionBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionBudgetStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionBudgetStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of position budget workflow.</summary>
public static class PositionBudgetWorkflow
{
    public static PositionBudgetCoordinator NewPositionBudgetCoordinator(IPositionBudgetStore store, IClock clock)
    {
        return new PositionBudgetCoordinator(
            store,
            PositionBudgetRules.DefaultPositionBudgetValidator(),
            clock,
            new List<string>());
    }
}
