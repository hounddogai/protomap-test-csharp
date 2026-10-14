namespace Acme.Workforce.Contractors.Engagement.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves engagement rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementRollupWorkflow(IEngagementRollupStore Store, EngagementRollupValidator Validator, IClock Clock, List<string> Journal)
{
    public static EngagementRollupWorkflow NewEngagementRollupWorkflow(IEngagementRollupStore store, IClock clock)
    {
        return new EngagementRollupWorkflow(
            store,
            EngagementRollupRules.DefaultEngagementRollupValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementRollupStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementRollupStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
