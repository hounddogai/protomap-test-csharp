namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshPreferenceCoordinator(IRefreshPreferenceStore Store, RefreshPreferenceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshPreferenceStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshPreferenceStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of refresh preference workflow.</summary>
public static class RefreshPreferenceWorkflow
{
    public static RefreshPreferenceCoordinator NewRefreshPreferenceCoordinator(IRefreshPreferenceStore store, IClock clock)
    {
        return new RefreshPreferenceCoordinator(
            store,
            RefreshPreferenceRules.DefaultRefreshPreferenceValidator(),
            clock,
            new List<string>());
    }
}
