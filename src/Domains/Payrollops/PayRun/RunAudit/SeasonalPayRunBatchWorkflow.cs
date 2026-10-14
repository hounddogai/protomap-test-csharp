namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal pay run batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalPayRunBatchService(ISeasonalPayRunBatchStore Store, SeasonalPayRunBatchValidator Validator, IClock Clock, List<string> Journal, SeasonalPayRunBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pay run batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalPayRunBatchStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pay run batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalPayRunBatchStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pay run batch {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of seasonal pay run batch workflow.</summary>
public static class SeasonalPayRunBatchWorkflow
{
    public static SeasonalPayRunBatchService NewSeasonalPayRunBatchService(ISeasonalPayRunBatchStore store, IClock clock, Registry flags)
    {
        return new SeasonalPayRunBatchService(
            store,
            SeasonalPayRunBatchRules.DefaultSeasonalPayRunBatchValidator(),
            clock,
            new List<string>(),
            SeasonalPayRunBatchEngine.DefaultSeasonalPayRunBatchEngine(flags));
    }
}
