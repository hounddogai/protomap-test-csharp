namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional deduction batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalDeductionBatchManager(IRegionalDeductionBatchStore Store, RegionalDeductionBatchValidator Validator, IClock Clock, List<string> Journal, RegionalDeductionBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional deduction batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalDeductionBatchStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional deduction batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalDeductionBatchStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional deduction batch {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of regional deduction batch workflow.</summary>
public static class RegionalDeductionBatchWorkflow
{
    public static RegionalDeductionBatchManager NewRegionalDeductionBatchManager(IRegionalDeductionBatchStore store, IClock clock, Registry flags)
    {
        return new RegionalDeductionBatchManager(
            store,
            RegionalDeductionBatchRules.DefaultRegionalDeductionBatchValidator(),
            clock,
            new List<string>(),
            RegionalDeductionBatchEngine.DefaultRegionalDeductionBatchEngine(flags));
    }
}
