namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves engagement policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementPolicyService(IEngagementPolicyStore Store, EngagementPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementPolicyStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementPolicyStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of engagement policy workflow.</summary>
public static class EngagementPolicyWorkflow
{
    public static EngagementPolicyService NewEngagementPolicyService(IEngagementPolicyStore store, IClock clock)
    {
        return new EngagementPolicyService(
            store,
            EngagementPolicyRules.DefaultEngagementPolicyValidator(),
            clock,
            new List<string>());
    }
}
