namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanBudgetCoordinator(ISpanBudgetStore Store, SpanBudgetValidator Validator, IClock Clock, List<string> Journal, SpanBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanBudgetStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanBudgetStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span budget {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of span budget workflow.</summary>
public static class SpanBudgetWorkflow
{
    public static SpanBudgetCoordinator NewSpanBudgetCoordinator(ISpanBudgetStore store, IClock clock)
    {
        return new SpanBudgetCoordinator(
            store,
            SpanBudgetRules.DefaultSpanBudgetValidator(),
            clock,
            new List<string>(),
            SpanBudgetEngine.DefaultSpanBudgetEngine());
    }
}
