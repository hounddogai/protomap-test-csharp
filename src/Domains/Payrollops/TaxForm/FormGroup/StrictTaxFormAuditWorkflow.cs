namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict tax form audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictTaxFormAuditService(IStrictTaxFormAuditStore Store, StrictTaxFormAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict tax form audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictTaxFormAuditStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict tax form audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictTaxFormAuditStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of strict tax form audit workflow.</summary>
public static class StrictTaxFormAuditWorkflow
{
    public static StrictTaxFormAuditService NewStrictTaxFormAuditService(IStrictTaxFormAuditStore store, IClock clock)
    {
        return new StrictTaxFormAuditService(
            store,
            StrictTaxFormAuditRules.DefaultStrictTaxFormAuditValidator(),
            clock,
            new List<string>());
    }
}
