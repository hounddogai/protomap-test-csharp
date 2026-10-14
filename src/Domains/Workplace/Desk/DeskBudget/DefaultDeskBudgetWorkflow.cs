namespace Acme.Workforce.Workplace.Desk.DeskBudget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default desk budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultDeskBudgetDesk(IDefaultDeskBudgetStore Store, DefaultDeskBudgetValidator Validator, IClock Clock, List<string> Journal, DefaultDeskBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default desk budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultDeskBudgetStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default desk budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultDeskBudgetStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default desk budget {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of default desk budget workflow.</summary>
public static class DefaultDeskBudgetWorkflow
{
    public static DefaultDeskBudgetDesk NewDefaultDeskBudgetDesk(IDefaultDeskBudgetStore store, IClock clock)
    {
        return new DefaultDeskBudgetDesk(
            store,
            DefaultDeskBudgetRules.DefaultDefaultDeskBudgetValidator(),
            clock,
            new List<string>(),
            DefaultDeskBudgetEngine.DefaultDefaultDeskBudgetEngine());
    }
}
