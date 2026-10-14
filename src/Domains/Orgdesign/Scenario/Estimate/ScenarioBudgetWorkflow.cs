namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scenario budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioBudgetCoordinator(IScenarioBudgetStore Store, ScenarioBudgetValidator Validator, IClock Clock, List<string> Journal, ScenarioBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioBudgetStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioBudgetStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario budget {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of scenario budget workflow.</summary>
public static class ScenarioBudgetWorkflow
{
    public static ScenarioBudgetCoordinator NewScenarioBudgetCoordinator(IScenarioBudgetStore store, IClock clock, Registry flags)
    {
        return new ScenarioBudgetCoordinator(
            store,
            ScenarioBudgetRules.DefaultScenarioBudgetValidator(),
            clock,
            new List<string>(),
            ScenarioBudgetEngine.DefaultScenarioBudgetEngine(flags));
    }
}
