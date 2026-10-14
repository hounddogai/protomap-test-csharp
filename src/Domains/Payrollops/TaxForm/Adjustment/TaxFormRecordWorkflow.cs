namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form records through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormRecordCoordinator(ITaxFormRecordStore Store, TaxFormRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormRecordStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormRecordStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of tax form record workflow.</summary>
public static class TaxFormRecordWorkflow
{
    public static TaxFormRecordCoordinator NewTaxFormRecordCoordinator(ITaxFormRecordStore store, IClock clock)
    {
        return new TaxFormRecordCoordinator(
            store,
            TaxFormRecordRules.DefaultTaxFormRecordValidator(),
            clock,
            new List<string>());
    }
}
