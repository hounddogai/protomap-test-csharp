namespace Acme.Workforce.Talent.Pool.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive pool assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptivePoolAssignmentDesk(IAdaptivePoolAssignmentStore Store, AdaptivePoolAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive pool assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptivePoolAssignmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive pool assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptivePoolAssignmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of adaptive pool assignment workflow.</summary>
public static class AdaptivePoolAssignmentWorkflow
{
    public static AdaptivePoolAssignmentDesk NewAdaptivePoolAssignmentDesk(IAdaptivePoolAssignmentStore store, IClock clock)
    {
        return new AdaptivePoolAssignmentDesk(
            store,
            AdaptivePoolAssignmentRules.DefaultAdaptivePoolAssignmentValidator(),
            clock,
            new List<string>());
    }
}
