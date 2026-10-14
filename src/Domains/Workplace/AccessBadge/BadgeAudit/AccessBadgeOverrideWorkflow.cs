namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves access badge overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeOverrideService(IAccessBadgeOverrideStore Store, AccessBadgeOverrideValidator Validator, IClock Clock, List<string> Journal, AccessBadgeOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeOverrideStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeOverrideStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge override {code} not found");
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

/// <summary>Functions of access badge override workflow.</summary>
public static class AccessBadgeOverrideWorkflow
{
    public static AccessBadgeOverrideService NewAccessBadgeOverrideService(IAccessBadgeOverrideStore store, IClock clock, Registry flags)
    {
        return new AccessBadgeOverrideService(
            store,
            AccessBadgeOverrideRules.DefaultAccessBadgeOverrideValidator(),
            clock,
            new List<string>(),
            AccessBadgeOverrideEngine.DefaultAccessBadgeOverrideEngine(flags));
    }
}
