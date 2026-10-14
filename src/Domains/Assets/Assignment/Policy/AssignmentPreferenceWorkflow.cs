namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves assignment preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentPreferenceCoordinator(IAssignmentPreferenceStore Store, AssignmentPreferenceValidator Validator, IClock Clock, List<string> Journal, AssignmentPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentPreferenceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentPreferenceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment preference {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of assignment preference workflow.</summary>
public static class AssignmentPreferenceWorkflow
{
    public static AssignmentPreferenceCoordinator NewAssignmentPreferenceCoordinator(IAssignmentPreferenceStore store, IClock clock)
    {
        return new AssignmentPreferenceCoordinator(
            store,
            AssignmentPreferenceRules.DefaultAssignmentPreferenceValidator(),
            clock,
            new List<string>(),
            AssignmentPreferenceEngine.DefaultAssignmentPreferenceEngine());
    }
}
