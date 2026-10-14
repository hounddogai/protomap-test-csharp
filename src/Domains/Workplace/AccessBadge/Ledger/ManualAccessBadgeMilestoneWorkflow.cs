namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual access badge milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualAccessBadgeMilestoneCoordinator(IManualAccessBadgeMilestoneStore Store, ManualAccessBadgeMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual access badge milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualAccessBadgeMilestoneStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual access badge milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualAccessBadgeMilestoneStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of manual access badge milestone workflow.</summary>
public static class ManualAccessBadgeMilestoneWorkflow
{
    public static ManualAccessBadgeMilestoneCoordinator NewManualAccessBadgeMilestoneCoordinator(IManualAccessBadgeMilestoneStore store, IClock clock)
    {
        return new ManualAccessBadgeMilestoneCoordinator(
            store,
            ManualAccessBadgeMilestoneRules.DefaultManualAccessBadgeMilestoneValidator(),
            clock,
            new List<string>());
    }
}
