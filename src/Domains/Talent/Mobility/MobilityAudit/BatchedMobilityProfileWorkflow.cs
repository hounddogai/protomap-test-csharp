namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched mobility profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedMobilityProfileWorkflow(IBatchedMobilityProfileStore Store, BatchedMobilityProfileValidator Validator, IClock Clock, List<string> Journal)
{
    public static BatchedMobilityProfileWorkflow NewBatchedMobilityProfileWorkflow(IBatchedMobilityProfileStore store, IClock clock)
    {
        return new BatchedMobilityProfileWorkflow(
            store,
            BatchedMobilityProfileRules.DefaultBatchedMobilityProfileValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched mobility profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedMobilityProfileStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched mobility profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedMobilityProfileStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public int Scan(int cutoff)
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
