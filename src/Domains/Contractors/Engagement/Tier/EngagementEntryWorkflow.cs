namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves engagement entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementEntryService(IEngagementEntryStore Store, EngagementEntryValidator Validator, IClock Clock, List<string> Journal, EngagementEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementEntryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementEntryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement entry {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of engagement entry workflow.</summary>
public static class EngagementEntryWorkflow
{
    public static EngagementEntryService NewEngagementEntryService(IEngagementEntryStore store, IClock clock, Registry flags)
    {
        return new EngagementEntryService(
            store,
            EngagementEntryRules.DefaultEngagementEntryValidator(),
            clock,
            new List<string>(),
            EngagementEntryEngine.DefaultEngagementEntryEngine(flags));
    }
}
