namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive engagement variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveEngagementVarianceDesk(IAdaptiveEngagementVarianceStore Store, AdaptiveEngagementVarianceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive engagement variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveEngagementVarianceStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive engagement variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveEngagementVarianceStatus.Retired);
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

/// <summary>Functions of adaptive engagement variance workflow.</summary>
public static class AdaptiveEngagementVarianceWorkflow
{
    public static AdaptiveEngagementVarianceDesk NewAdaptiveEngagementVarianceDesk(IAdaptiveEngagementVarianceStore store, IClock clock)
    {
        return new AdaptiveEngagementVarianceDesk(
            store,
            AdaptiveEngagementVarianceRules.DefaultAdaptiveEngagementVarianceValidator(),
            clock,
            new List<string>());
    }
}
