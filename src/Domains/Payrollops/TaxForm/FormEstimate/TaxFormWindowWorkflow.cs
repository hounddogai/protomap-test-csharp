namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormWindowDesk(ITaxFormWindowStore Store, TaxFormWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormWindowStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormWindowStatus.Cancelled);
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

/// <summary>Functions of tax form window workflow.</summary>
public static class TaxFormWindowWorkflow
{
    public static TaxFormWindowDesk NewTaxFormWindowDesk(ITaxFormWindowStore store, IClock clock)
    {
        return new TaxFormWindowDesk(
            store,
            TaxFormWindowRules.DefaultTaxFormWindowValidator(),
            clock,
            new List<string>());
    }
}
