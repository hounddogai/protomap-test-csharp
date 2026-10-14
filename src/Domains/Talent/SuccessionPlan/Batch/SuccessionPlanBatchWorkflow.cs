namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves succession plan batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record SuccessionPlanBatchService(ISuccessionPlanBatchStore Store, SuccessionPlanBatchValidator Validator, IClock Clock, List<string> Journal, SuccessionPlanBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanBatchStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanBatchStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan batch {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of succession plan batch workflow.</summary>
public static class SuccessionPlanBatchWorkflow
{
    public static SuccessionPlanBatchService NewSuccessionPlanBatchService(ISuccessionPlanBatchStore store, IClock clock)
    {
        return new SuccessionPlanBatchService(
            store,
            SuccessionPlanBatchRules.DefaultSuccessionPlanBatchValidator(),
            clock,
            new List<string>(),
            SuccessionPlanBatchEngine.DefaultSuccessionPlanBatchEngine());
    }
}
