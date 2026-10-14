namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves engagement groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementGroupService(IEngagementGroupStore Store, EngagementGroupValidator Validator, IClock Clock, List<string> Journal, EngagementGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementGroupStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementGroupStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement group {code} not found");
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

/// <summary>Functions of engagement group workflow.</summary>
public static class EngagementGroupWorkflow
{
    public static EngagementGroupService NewEngagementGroupService(IEngagementGroupStore store, IClock clock)
    {
        return new EngagementGroupService(
            store,
            EngagementGroupRules.DefaultEngagementGroupValidator(),
            clock,
            new List<string>(),
            EngagementGroupEngine.DefaultEngagementGroupEngine());
    }
}
