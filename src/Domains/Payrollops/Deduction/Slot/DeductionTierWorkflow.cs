namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves deduction tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionTierDesk(IDeductionTierStore Store, DeductionTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionTierStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionTierStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of deduction tier workflow.</summary>
public static class DeductionTierWorkflow
{
    public static DeductionTierDesk NewDeductionTierDesk(IDeductionTierStore store, IClock clock)
    {
        return new DeductionTierDesk(
            store,
            DeductionTierRules.DefaultDeductionTierValidator(),
            clock,
            new List<string>());
    }
}
