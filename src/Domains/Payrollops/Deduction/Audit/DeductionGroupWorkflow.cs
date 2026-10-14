namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves deduction groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionGroupDesk(IDeductionGroupStore Store, DeductionGroupValidator Validator, IClock Clock, List<string> Journal, DeductionGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionGroupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionGroupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction group {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of deduction group workflow.</summary>
public static class DeductionGroupWorkflow
{
    public static DeductionGroupDesk NewDeductionGroupDesk(IDeductionGroupStore store, IClock clock, Registry flags)
    {
        return new DeductionGroupDesk(
            store,
            DeductionGroupRules.DefaultDeductionGroupValidator(),
            clock,
            new List<string>(),
            DeductionGroupEngine.DefaultDeductionGroupEngine(flags));
    }
}
