namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves deduction estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionEstimateManager(IDeductionEstimateStore Store, DeductionEstimateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionEstimateStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionEstimateStatus.Cancelled);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of deduction estimate workflow.</summary>
public static class DeductionEstimateWorkflow
{
    public static DeductionEstimateManager NewDeductionEstimateManager(IDeductionEstimateStore store, IClock clock)
    {
        return new DeductionEstimateManager(
            store,
            DeductionEstimateRules.DefaultDeductionEstimateValidator(),
            clock,
            new List<string>());
    }
}
