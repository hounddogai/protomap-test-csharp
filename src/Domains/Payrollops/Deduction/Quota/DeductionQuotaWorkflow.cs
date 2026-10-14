namespace Acme.Workforce.Payrollops.Deduction.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves deduction quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionQuotaCoordinator(IDeductionQuotaStore Store, DeductionQuotaValidator Validator, IClock Clock, List<string> Journal, DeductionQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionQuotaStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionQuotaStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction quota {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of deduction quota workflow.</summary>
public static class DeductionQuotaWorkflow
{
    public static DeductionQuotaCoordinator NewDeductionQuotaCoordinator(IDeductionQuotaStore store, IClock clock)
    {
        return new DeductionQuotaCoordinator(
            store,
            DeductionQuotaRules.DefaultDeductionQuotaValidator(),
            clock,
            new List<string>(),
            DeductionQuotaEngine.DefaultDeductionQuotaEngine());
    }
}
