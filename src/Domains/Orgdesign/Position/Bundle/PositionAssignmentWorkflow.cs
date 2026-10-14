namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionAssignmentDesk(IPositionAssignmentStore Store, PositionAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionAssignmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionAssignmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of position assignment workflow.</summary>
public static class PositionAssignmentWorkflow
{
    public static PositionAssignmentDesk NewPositionAssignmentDesk(IPositionAssignmentStore store, IClock clock)
    {
        return new PositionAssignmentDesk(
            store,
            PositionAssignmentRules.DefaultPositionAssignmentValidator(),
            clock,
            new List<string>());
    }
}
