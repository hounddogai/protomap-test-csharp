namespace Acme.Workforce.Payrollops.TaxForm.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tax form cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormCycleDesk(ITaxFormCycleStore Store, TaxFormCycleValidator Validator, IClock Clock, List<string> Journal, TaxFormCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormCycleStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormCycleStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form cycle {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of tax form cycle workflow.</summary>
public static class TaxFormCycleWorkflow
{
    public static TaxFormCycleDesk NewTaxFormCycleDesk(ITaxFormCycleStore store, IClock clock, Registry flags)
    {
        return new TaxFormCycleDesk(
            store,
            TaxFormCycleRules.DefaultTaxFormCycleValidator(),
            clock,
            new List<string>(),
            TaxFormCycleEngine.DefaultTaxFormCycleEngine(flags));
    }
}
