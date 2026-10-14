namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped access badge drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedAccessBadgeDraftWorkflow(ICappedAccessBadgeDraftStore Store, CappedAccessBadgeDraftValidator Validator, IClock Clock, List<string> Journal, CappedAccessBadgeDraftEngine Engine)
{
    public static CappedAccessBadgeDraftWorkflow NewCappedAccessBadgeDraftWorkflow(ICappedAccessBadgeDraftStore store, IClock clock)
    {
        return new CappedAccessBadgeDraftWorkflow(
            store,
            CappedAccessBadgeDraftRules.DefaultCappedAccessBadgeDraftValidator(),
            clock,
            new List<string>(),
            CappedAccessBadgeDraftEngine.DefaultCappedAccessBadgeDraftEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped access badge draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedAccessBadgeDraftStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped access badge draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedAccessBadgeDraftStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped access badge draft {code} not found");
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
