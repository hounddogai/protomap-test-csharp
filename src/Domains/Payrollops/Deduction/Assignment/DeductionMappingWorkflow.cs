namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves deduction mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionMappingCoordinator(IDeductionMappingStore Store, DeductionMappingValidator Validator, IClock Clock, List<string> Journal, DeductionMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionMappingStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionMappingStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction mapping {code} not found");
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

/// <summary>Functions of deduction mapping workflow.</summary>
public static class DeductionMappingWorkflow
{
    public static DeductionMappingCoordinator NewDeductionMappingCoordinator(IDeductionMappingStore store, IClock clock, Registry flags)
    {
        return new DeductionMappingCoordinator(
            store,
            DeductionMappingRules.DefaultDeductionMappingValidator(),
            clock,
            new List<string>(),
            DeductionMappingEngine.DefaultDeductionMappingEngine(flags));
    }
}
