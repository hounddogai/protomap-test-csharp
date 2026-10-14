namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled policy items through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledPolicyItemService(IScheduledPolicyItemStore Store, ScheduledPolicyItemValidator Validator, IClock Clock, List<string> Journal, ScheduledPolicyItemEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled policy item {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPolicyItemStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled policy item {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPolicyItemStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled policy item {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of scheduled policy item workflow.</summary>
public static class ScheduledPolicyItemWorkflow
{
    public static ScheduledPolicyItemService NewScheduledPolicyItemService(IScheduledPolicyItemStore store, IClock clock, Registry flags)
    {
        return new ScheduledPolicyItemService(
            store,
            ScheduledPolicyItemRules.DefaultScheduledPolicyItemValidator(),
            clock,
            new List<string>(),
            ScheduledPolicyItemEngine.DefaultScheduledPolicyItemEngine(flags));
    }
}
