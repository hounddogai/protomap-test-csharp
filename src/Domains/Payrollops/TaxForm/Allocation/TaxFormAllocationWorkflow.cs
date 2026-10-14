namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormAllocationCoordinator(ITaxFormAllocationStore Store, TaxFormAllocationValidator Validator, IClock Clock, List<string> Journal, TaxFormAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormAllocationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormAllocationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form allocation {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of tax form allocation workflow.</summary>
public static class TaxFormAllocationWorkflow
{
    public static TaxFormAllocationCoordinator NewTaxFormAllocationCoordinator(ITaxFormAllocationStore store, IClock clock)
    {
        return new TaxFormAllocationCoordinator(
            store,
            TaxFormAllocationRules.DefaultTaxFormAllocationValidator(),
            clock,
            new List<string>(),
            TaxFormAllocationEngine.DefaultTaxFormAllocationEngine());
    }
}
