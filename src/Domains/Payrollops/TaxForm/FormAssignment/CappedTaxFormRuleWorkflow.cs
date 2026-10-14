namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped tax form rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedTaxFormRuleDesk(ICappedTaxFormRuleStore Store, CappedTaxFormRuleValidator Validator, IClock Clock, List<string> Journal, CappedTaxFormRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped tax form rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedTaxFormRuleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped tax form rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedTaxFormRuleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped tax form rule {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of capped tax form rule workflow.</summary>
public static class CappedTaxFormRuleWorkflow
{
    public static CappedTaxFormRuleDesk NewCappedTaxFormRuleDesk(ICappedTaxFormRuleStore store, IClock clock)
    {
        return new CappedTaxFormRuleDesk(
            store,
            CappedTaxFormRuleRules.DefaultCappedTaxFormRuleValidator(),
            clock,
            new List<string>(),
            CappedTaxFormRuleEngine.DefaultCappedTaxFormRuleEngine());
    }
}
