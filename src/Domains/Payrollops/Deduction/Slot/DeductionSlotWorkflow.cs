namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves deduction slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionSlotDesk(IDeductionSlotStore Store, DeductionSlotValidator Validator, IClock Clock, List<string> Journal, DeductionSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionSlotStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionSlotStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction slot {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of deduction slot workflow.</summary>
public static class DeductionSlotWorkflow
{
    public static DeductionSlotDesk NewDeductionSlotDesk(IDeductionSlotStore store, IClock clock)
    {
        return new DeductionSlotDesk(
            store,
            DeductionSlotRules.DefaultDeductionSlotValidator(),
            clock,
            new List<string>(),
            DeductionSlotEngine.DefaultDeductionSlotEngine());
    }
}
