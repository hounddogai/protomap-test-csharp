namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves assignment windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentWindowDesk(IAssignmentWindowStore Store, AssignmentWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentWindowStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentWindowStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of assignment window workflow.</summary>
public static class AssignmentWindowWorkflow
{
    public static AssignmentWindowDesk NewAssignmentWindowDesk(IAssignmentWindowStore store, IClock clock)
    {
        return new AssignmentWindowDesk(
            store,
            AssignmentWindowRules.DefaultAssignmentWindowValidator(),
            clock,
            new List<string>());
    }
}
