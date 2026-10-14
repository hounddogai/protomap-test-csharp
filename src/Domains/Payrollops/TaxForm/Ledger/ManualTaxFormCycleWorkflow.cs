namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual tax form cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualTaxFormCycleService(IManualTaxFormCycleStore Store, ManualTaxFormCycleValidator Validator, IClock Clock, List<string> Journal, ManualTaxFormCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual tax form cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualTaxFormCycleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual tax form cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualTaxFormCycleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual tax form cycle {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of manual tax form cycle workflow.</summary>
public static class ManualTaxFormCycleWorkflow
{
    public static ManualTaxFormCycleService NewManualTaxFormCycleService(IManualTaxFormCycleStore store, IClock clock)
    {
        return new ManualTaxFormCycleService(
            store,
            ManualTaxFormCycleRules.DefaultManualTaxFormCycleValidator(),
            clock,
            new List<string>(),
            ManualTaxFormCycleEngine.DefaultManualTaxFormCycleEngine());
    }
}
