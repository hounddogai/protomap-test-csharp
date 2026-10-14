namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormEntryManager(ITaxFormEntryStore Store, TaxFormEntryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormEntryStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormEntryStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of tax form entry workflow.</summary>
public static class TaxFormEntryWorkflow
{
    public static TaxFormEntryManager NewTaxFormEntryManager(ITaxFormEntryStore store, IClock clock)
    {
        return new TaxFormEntryManager(
            store,
            TaxFormEntryRules.DefaultTaxFormEntryValidator(),
            clock,
            new List<string>());
    }
}
