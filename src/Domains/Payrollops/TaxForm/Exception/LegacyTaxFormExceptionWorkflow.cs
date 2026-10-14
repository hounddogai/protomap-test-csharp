namespace Acme.Workforce.Payrollops.TaxForm.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy tax form exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyTaxFormExceptionService(ILegacyTaxFormExceptionStore Store, LegacyTaxFormExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy tax form exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyTaxFormExceptionStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy tax form exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyTaxFormExceptionStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of legacy tax form exception workflow.</summary>
public static class LegacyTaxFormExceptionWorkflow
{
    public static LegacyTaxFormExceptionService NewLegacyTaxFormExceptionService(ILegacyTaxFormExceptionStore store, IClock clock)
    {
        return new LegacyTaxFormExceptionService(
            store,
            LegacyTaxFormExceptionRules.DefaultLegacyTaxFormExceptionValidator(),
            clock,
            new List<string>());
    }
}
