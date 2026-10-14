namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves exception budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionBudgetCoordinator(IExceptionBudgetStore Store, ExceptionBudgetValidator Validator, IClock Clock, List<string> Journal, ExceptionBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionBudgetStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionBudgetStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception budget {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of exception budget workflow.</summary>
public static class ExceptionBudgetWorkflow
{
    public static ExceptionBudgetCoordinator NewExceptionBudgetCoordinator(IExceptionBudgetStore store, IClock clock)
    {
        return new ExceptionBudgetCoordinator(
            store,
            ExceptionBudgetRules.DefaultExceptionBudgetValidator(),
            clock,
            new List<string>(),
            ExceptionBudgetEngine.DefaultExceptionBudgetEngine());
    }
}
