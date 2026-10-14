namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual tax form rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualTaxFormRollupManager(IManualTaxFormRollupStore Store, ManualTaxFormRollupValidator Validator, IClock Clock, List<string> Journal, ManualTaxFormRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual tax form rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualTaxFormRollupStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual tax form rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualTaxFormRollupStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual tax form rollup {code} not found");
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

/// <summary>Functions of manual tax form rollup workflow.</summary>
public static class ManualTaxFormRollupWorkflow
{
    public static ManualTaxFormRollupManager NewManualTaxFormRollupManager(IManualTaxFormRollupStore store, IClock clock, Registry flags)
    {
        return new ManualTaxFormRollupManager(
            store,
            ManualTaxFormRollupRules.DefaultManualTaxFormRollupValidator(),
            clock,
            new List<string>(),
            ManualTaxFormRollupEngine.DefaultManualTaxFormRollupEngine(flags));
    }
}
