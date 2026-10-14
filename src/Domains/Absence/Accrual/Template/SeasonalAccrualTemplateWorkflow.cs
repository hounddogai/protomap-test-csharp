namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal accrual templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalAccrualTemplateDesk(ISeasonalAccrualTemplateStore Store, SeasonalAccrualTemplateValidator Validator, IClock Clock, List<string> Journal, SeasonalAccrualTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal accrual template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAccrualTemplateStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal accrual template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAccrualTemplateStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal accrual template {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of seasonal accrual template workflow.</summary>
public static class SeasonalAccrualTemplateWorkflow
{
    public static SeasonalAccrualTemplateDesk NewSeasonalAccrualTemplateDesk(ISeasonalAccrualTemplateStore store, IClock clock)
    {
        return new SeasonalAccrualTemplateDesk(
            store,
            SeasonalAccrualTemplateRules.DefaultSeasonalAccrualTemplateValidator(),
            clock,
            new List<string>(),
            SeasonalAccrualTemplateEngine.DefaultSeasonalAccrualTemplateEngine());
    }
}
