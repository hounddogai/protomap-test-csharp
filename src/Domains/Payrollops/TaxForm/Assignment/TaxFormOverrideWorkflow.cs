namespace Acme.Workforce.Payrollops.TaxForm.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormOverrideDesk(ITaxFormOverrideStore Store, TaxFormOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormOverrideStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormOverrideStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of tax form override workflow.</summary>
public static class TaxFormOverrideWorkflow
{
    public static TaxFormOverrideDesk NewTaxFormOverrideDesk(ITaxFormOverrideStore store, IClock clock)
    {
        return new TaxFormOverrideDesk(
            store,
            TaxFormOverrideRules.DefaultTaxFormOverrideValidator(),
            clock,
            new List<string>());
    }
}
