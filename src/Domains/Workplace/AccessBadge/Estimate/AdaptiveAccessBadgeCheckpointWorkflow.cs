namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive access badge checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveAccessBadgeCheckpointDesk(IAdaptiveAccessBadgeCheckpointStore Store, AdaptiveAccessBadgeCheckpointValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive access badge checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveAccessBadgeCheckpointStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive access badge checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveAccessBadgeCheckpointStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of adaptive access badge checkpoint workflow.</summary>
public static class AdaptiveAccessBadgeCheckpointWorkflow
{
    public static AdaptiveAccessBadgeCheckpointDesk NewAdaptiveAccessBadgeCheckpointDesk(IAdaptiveAccessBadgeCheckpointStore store, IClock clock)
    {
        return new AdaptiveAccessBadgeCheckpointDesk(
            store,
            AdaptiveAccessBadgeCheckpointRules.DefaultAdaptiveAccessBadgeCheckpointValidator(),
            clock,
            new List<string>());
    }
}
