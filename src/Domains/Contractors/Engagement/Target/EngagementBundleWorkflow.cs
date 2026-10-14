namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves engagement bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementBundleService(IEngagementBundleStore Store, EngagementBundleValidator Validator, IClock Clock, List<string> Journal, EngagementBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementBundleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementBundleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement bundle {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of engagement bundle workflow.</summary>
public static class EngagementBundleWorkflow
{
    public static EngagementBundleService NewEngagementBundleService(IEngagementBundleStore store, IClock clock, Registry flags)
    {
        return new EngagementBundleService(
            store,
            EngagementBundleRules.DefaultEngagementBundleValidator(),
            clock,
            new List<string>(),
            EngagementBundleEngine.DefaultEngagementBundleEngine(flags));
    }
}
