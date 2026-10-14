namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict tax form tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictTaxFormTierWorkflow(IStrictTaxFormTierStore Store, StrictTaxFormTierValidator Validator, IClock Clock, List<string> Journal)
{
    public static StrictTaxFormTierWorkflow NewStrictTaxFormTierWorkflow(IStrictTaxFormTierStore store, IClock clock)
    {
        return new StrictTaxFormTierWorkflow(
            store,
            StrictTaxFormTierRules.DefaultStrictTaxFormTierValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict tax form tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictTaxFormTierStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict tax form tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictTaxFormTierStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
