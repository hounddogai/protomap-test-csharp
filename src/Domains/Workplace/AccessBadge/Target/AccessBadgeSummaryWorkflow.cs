namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeSummaryService(IAccessBadgeSummaryStore Store, AccessBadgeSummaryValidator Validator, IClock Clock, List<string> Journal, AccessBadgeSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeSummaryStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeSummaryStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge summary {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of access badge summary workflow.</summary>
public static class AccessBadgeSummaryWorkflow
{
    public static AccessBadgeSummaryService NewAccessBadgeSummaryService(IAccessBadgeSummaryStore store, IClock clock)
    {
        return new AccessBadgeSummaryService(
            store,
            AccessBadgeSummaryRules.DefaultAccessBadgeSummaryValidator(),
            clock,
            new List<string>(),
            AccessBadgeSummaryEngine.DefaultAccessBadgeSummaryEngine());
    }
}
