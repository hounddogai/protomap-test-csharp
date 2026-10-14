namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormRuleManager(ITaxFormRuleStore Store, TaxFormRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormRuleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormRuleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of tax form rule workflow.</summary>
public static class TaxFormRuleWorkflow
{
    public static TaxFormRuleManager NewTaxFormRuleManager(ITaxFormRuleStore store, IClock clock)
    {
        return new TaxFormRuleManager(
            store,
            TaxFormRuleRules.DefaultTaxFormRuleValidator(),
            clock,
            new List<string>());
    }
}
