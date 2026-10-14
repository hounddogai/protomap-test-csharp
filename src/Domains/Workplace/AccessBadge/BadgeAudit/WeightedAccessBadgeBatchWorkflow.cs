namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted access badge batches through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedAccessBadgeBatchDesk(IWeightedAccessBadgeBatchStore Store, WeightedAccessBadgeBatchValidator Validator, IClock Clock, List<string> Journal, WeightedAccessBadgeBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted access badge batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedAccessBadgeBatchStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted access badge batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedAccessBadgeBatchStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted access badge batch {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of weighted access badge batch workflow.</summary>
public static class WeightedAccessBadgeBatchWorkflow
{
    public static WeightedAccessBadgeBatchDesk NewWeightedAccessBadgeBatchDesk(IWeightedAccessBadgeBatchStore store, IClock clock)
    {
        return new WeightedAccessBadgeBatchDesk(
            store,
            WeightedAccessBadgeBatchRules.DefaultWeightedAccessBadgeBatchValidator(),
            clock,
            new List<string>(),
            WeightedAccessBadgeBatchEngine.DefaultWeightedAccessBadgeBatchEngine());
    }
}
