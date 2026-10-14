namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormClaimDesk(ITaxFormClaimStore Store, TaxFormClaimValidator Validator, IClock Clock, List<string> Journal, TaxFormClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormClaimStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormClaimStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form claim {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of tax form claim workflow.</summary>
public static class TaxFormClaimWorkflow
{
    public static TaxFormClaimDesk NewTaxFormClaimDesk(ITaxFormClaimStore store, IClock clock)
    {
        return new TaxFormClaimDesk(
            store,
            TaxFormClaimRules.DefaultTaxFormClaimValidator(),
            clock,
            new List<string>(),
            TaxFormClaimEngine.DefaultTaxFormClaimEngine());
    }
}
