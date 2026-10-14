namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormAgreementManager(ITaxFormAgreementStore Store, TaxFormAgreementValidator Validator, IClock Clock, List<string> Journal, TaxFormAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormAgreementStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormAgreementStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form agreement {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of tax form agreement workflow.</summary>
public static class TaxFormAgreementWorkflow
{
    public static TaxFormAgreementManager NewTaxFormAgreementManager(ITaxFormAgreementStore store, IClock clock)
    {
        return new TaxFormAgreementManager(
            store,
            TaxFormAgreementRules.DefaultTaxFormAgreementValidator(),
            clock,
            new List<string>(),
            TaxFormAgreementEngine.DefaultTaxFormAgreementEngine());
    }
}
