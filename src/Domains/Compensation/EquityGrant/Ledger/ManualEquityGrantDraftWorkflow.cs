namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual equity grant drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualEquityGrantDraftService(IManualEquityGrantDraftStore Store, ManualEquityGrantDraftValidator Validator, IClock Clock, List<string> Journal, ManualEquityGrantDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual equity grant draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualEquityGrantDraftStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual equity grant draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualEquityGrantDraftStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual equity grant draft {code} not found");
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

/// <summary>Functions of manual equity grant draft workflow.</summary>
public static class ManualEquityGrantDraftWorkflow
{
    public static ManualEquityGrantDraftService NewManualEquityGrantDraftService(IManualEquityGrantDraftStore store, IClock clock)
    {
        return new ManualEquityGrantDraftService(
            store,
            ManualEquityGrantDraftRules.DefaultManualEquityGrantDraftValidator(),
            clock,
            new List<string>(),
            ManualEquityGrantDraftEngine.DefaultManualEquityGrantDraftEngine());
    }
}
