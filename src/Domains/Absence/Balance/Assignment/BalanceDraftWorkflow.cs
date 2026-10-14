namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceDraftCoordinator(IBalanceDraftStore Store, BalanceDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceDraftStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceDraftStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of balance draft workflow.</summary>
public static class BalanceDraftWorkflow
{
    public static BalanceDraftCoordinator NewBalanceDraftCoordinator(IBalanceDraftStore store, IClock clock)
    {
        return new BalanceDraftCoordinator(
            store,
            BalanceDraftRules.DefaultBalanceDraftValidator(),
            clock,
            new List<string>());
    }
}
