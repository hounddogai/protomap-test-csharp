namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tax form ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormLedgerDesk(ITaxFormLedgerStore Store, TaxFormLedgerValidator Validator, IClock Clock, List<string> Journal, TaxFormLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormLedgerStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormLedgerStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form ledger {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of tax form ledger workflow.</summary>
public static class TaxFormLedgerWorkflow
{
    public static TaxFormLedgerDesk NewTaxFormLedgerDesk(ITaxFormLedgerStore store, IClock clock, Registry flags)
    {
        return new TaxFormLedgerDesk(
            store,
            TaxFormLedgerRules.DefaultTaxFormLedgerValidator(),
            clock,
            new List<string>(),
            TaxFormLedgerEngine.DefaultTaxFormLedgerEngine(flags));
    }
}
