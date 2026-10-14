namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeNoticeService(IAccessBadgeNoticeStore Store, AccessBadgeNoticeValidator Validator, IClock Clock, List<string> Journal, AccessBadgeNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeNoticeStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeNoticeStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge notice {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of access badge notice workflow.</summary>
public static class AccessBadgeNoticeWorkflow
{
    public static AccessBadgeNoticeService NewAccessBadgeNoticeService(IAccessBadgeNoticeStore store, IClock clock)
    {
        return new AccessBadgeNoticeService(
            store,
            AccessBadgeNoticeRules.DefaultAccessBadgeNoticeValidator(),
            clock,
            new List<string>(),
            AccessBadgeNoticeEngine.DefaultAccessBadgeNoticeEngine());
    }
}
