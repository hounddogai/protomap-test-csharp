namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormBaselineCoordinator(ITaxFormBaselineStore Store, TaxFormBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormBaselineStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormBaselineStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of tax form baseline workflow.</summary>
public static class TaxFormBaselineWorkflow
{
    public static TaxFormBaselineCoordinator NewTaxFormBaselineCoordinator(ITaxFormBaselineStore store, IClock clock)
    {
        return new TaxFormBaselineCoordinator(
            store,
            TaxFormBaselineRules.DefaultTaxFormBaselineValidator(),
            clock,
            new List<string>());
    }
}
