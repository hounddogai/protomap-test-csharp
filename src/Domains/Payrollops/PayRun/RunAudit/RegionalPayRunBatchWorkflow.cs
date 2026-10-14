namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional pay run batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalPayRunBatchDesk(IRegionalPayRunBatchStore Store, RegionalPayRunBatchValidator Validator, IClock Clock, List<string> Journal, RegionalPayRunBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pay run batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalPayRunBatchStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pay run batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalPayRunBatchStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pay run batch {code} not found");
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

/// <summary>Functions of regional pay run batch workflow.</summary>
public static class RegionalPayRunBatchWorkflow
{
    public static RegionalPayRunBatchDesk NewRegionalPayRunBatchDesk(IRegionalPayRunBatchStore store, IClock clock)
    {
        return new RegionalPayRunBatchDesk(
            store,
            RegionalPayRunBatchRules.DefaultRegionalPayRunBatchValidator(),
            clock,
            new List<string>(),
            RegionalPayRunBatchEngine.DefaultRegionalPayRunBatchEngine());
    }
}
