namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched headcount templates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedHeadcountTemplateService(IBatchedHeadcountTemplateStore Store, BatchedHeadcountTemplateValidator Validator, IClock Clock, List<string> Journal, BatchedHeadcountTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched headcount template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedHeadcountTemplateStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched headcount template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedHeadcountTemplateStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched headcount template {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of batched headcount template workflow.</summary>
public static class BatchedHeadcountTemplateWorkflow
{
    public static BatchedHeadcountTemplateService NewBatchedHeadcountTemplateService(IBatchedHeadcountTemplateStore store, IClock clock)
    {
        return new BatchedHeadcountTemplateService(
            store,
            BatchedHeadcountTemplateRules.DefaultBatchedHeadcountTemplateValidator(),
            clock,
            new List<string>(),
            BatchedHeadcountTemplateEngine.DefaultBatchedHeadcountTemplateEngine());
    }
}
