namespace Acme.Workforce.Assets.Assignment.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped assignment preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedAssignmentPreferenceManager(ICappedAssignmentPreferenceStore Store, CappedAssignmentPreferenceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped assignment preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedAssignmentPreferenceStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped assignment preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedAssignmentPreferenceStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of capped assignment preference workflow.</summary>
public static class CappedAssignmentPreferenceWorkflow
{
    public static CappedAssignmentPreferenceManager NewCappedAssignmentPreferenceManager(ICappedAssignmentPreferenceStore store, IClock clock)
    {
        return new CappedAssignmentPreferenceManager(
            store,
            CappedAssignmentPreferenceRules.DefaultCappedAssignmentPreferenceValidator(),
            clock,
            new List<string>());
    }
}
