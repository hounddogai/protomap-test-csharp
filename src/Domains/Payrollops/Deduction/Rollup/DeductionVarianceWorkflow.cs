namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves deduction variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionVarianceDesk(IDeductionVarianceStore Store, DeductionVarianceValidator Validator, IClock Clock, List<string> Journal, DeductionVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionVarianceStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionVarianceStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction variance {code} not found");
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

/// <summary>Functions of deduction variance workflow.</summary>
public static class DeductionVarianceWorkflow
{
    public static DeductionVarianceDesk NewDeductionVarianceDesk(IDeductionVarianceStore store, IClock clock, Registry flags)
    {
        return new DeductionVarianceDesk(
            store,
            DeductionVarianceRules.DefaultDeductionVarianceValidator(),
            clock,
            new List<string>(),
            DeductionVarianceEngine.DefaultDeductionVarianceEngine(flags));
    }
}
