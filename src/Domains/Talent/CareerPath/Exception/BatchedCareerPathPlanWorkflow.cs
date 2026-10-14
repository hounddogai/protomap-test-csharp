namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched career path plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedCareerPathPlanWorkflow(IBatchedCareerPathPlanStore Store, BatchedCareerPathPlanValidator Validator, IClock Clock, List<string> Journal)
{
    public static BatchedCareerPathPlanWorkflow NewBatchedCareerPathPlanWorkflow(IBatchedCareerPathPlanStore store, IClock clock)
    {
        return new BatchedCareerPathPlanWorkflow(
            store,
            BatchedCareerPathPlanRules.DefaultBatchedCareerPathPlanValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched career path plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedCareerPathPlanStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched career path plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedCareerPathPlanStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
            if (item.EndsAt > cutoff)
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
