namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal deduction quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalDeductionQuotaManager(ISeasonalDeductionQuotaStore Store, SeasonalDeductionQuotaValidator Validator, IClock Clock, List<string> Journal, SeasonalDeductionQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal deduction quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalDeductionQuotaStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal deduction quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalDeductionQuotaStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal deduction quota {code} not found");
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

/// <summary>Functions of seasonal deduction quota workflow.</summary>
public static class SeasonalDeductionQuotaWorkflow
{
    public static SeasonalDeductionQuotaManager NewSeasonalDeductionQuotaManager(ISeasonalDeductionQuotaStore store, IClock clock, Registry flags)
    {
        return new SeasonalDeductionQuotaManager(
            store,
            SeasonalDeductionQuotaRules.DefaultSeasonalDeductionQuotaValidator(),
            clock,
            new List<string>(),
            SeasonalDeductionQuotaEngine.DefaultSeasonalDeductionQuotaEngine(flags));
    }
}
