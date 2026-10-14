namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshProfileCoordinator(IRefreshProfileStore Store, RefreshProfileValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshProfileStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshProfileStatus.Closed);
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

/// <summary>Functions of refresh profile workflow.</summary>
public static class RefreshProfileWorkflow
{
    public static RefreshProfileCoordinator NewRefreshProfileCoordinator(IRefreshProfileStore store, IClock clock)
    {
        return new RefreshProfileCoordinator(
            store,
            RefreshProfileRules.DefaultRefreshProfileValidator(),
            clock,
            new List<string>());
    }
}
