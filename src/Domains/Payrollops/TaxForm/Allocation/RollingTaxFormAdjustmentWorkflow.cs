namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling tax form adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingTaxFormAdjustmentCoordinator(IRollingTaxFormAdjustmentStore Store, RollingTaxFormAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling tax form adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingTaxFormAdjustmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling tax form adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingTaxFormAdjustmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of rolling tax form adjustment workflow.</summary>
public static class RollingTaxFormAdjustmentWorkflow
{
    public static RollingTaxFormAdjustmentCoordinator NewRollingTaxFormAdjustmentCoordinator(IRollingTaxFormAdjustmentStore store, IClock clock)
    {
        return new RollingTaxFormAdjustmentCoordinator(
            store,
            RollingTaxFormAdjustmentRules.DefaultRollingTaxFormAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
