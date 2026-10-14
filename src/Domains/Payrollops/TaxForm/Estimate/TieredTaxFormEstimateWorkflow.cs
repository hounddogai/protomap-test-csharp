namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered tax form estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredTaxFormEstimateService(ITieredTaxFormEstimateStore Store, TieredTaxFormEstimateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered tax form estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredTaxFormEstimateStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered tax form estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredTaxFormEstimateStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of tiered tax form estimate workflow.</summary>
public static class TieredTaxFormEstimateWorkflow
{
    public static TieredTaxFormEstimateService NewTieredTaxFormEstimateService(ITieredTaxFormEstimateStore store, IClock clock)
    {
        return new TieredTaxFormEstimateService(
            store,
            TieredTaxFormEstimateRules.DefaultTieredTaxFormEstimateValidator(),
            clock,
            new List<string>());
    }
}
