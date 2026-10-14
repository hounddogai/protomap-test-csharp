namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched pay band templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedPayBandTemplateWorkflow(IBatchedPayBandTemplateStore Store, BatchedPayBandTemplateValidator Validator, IClock Clock, List<string> Journal, BatchedPayBandTemplateEngine Engine)
{
    public static BatchedPayBandTemplateWorkflow NewBatchedPayBandTemplateWorkflow(IBatchedPayBandTemplateStore store, IClock clock)
    {
        return new BatchedPayBandTemplateWorkflow(
            store,
            BatchedPayBandTemplateRules.DefaultBatchedPayBandTemplateValidator(),
            clock,
            new List<string>(),
            BatchedPayBandTemplateEngine.DefaultBatchedPayBandTemplateEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched pay band template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedPayBandTemplateStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched pay band template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedPayBandTemplateStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched pay band template {code} not found");
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
