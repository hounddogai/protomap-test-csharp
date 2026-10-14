namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionCycleCoordinator(IPositionCycleStore Store, PositionCycleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionCycleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionCycleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of position cycle workflow.</summary>
public static class PositionCycleWorkflow
{
    public static PositionCycleCoordinator NewPositionCycleCoordinator(IPositionCycleStore store, IClock clock)
    {
        return new PositionCycleCoordinator(
            store,
            PositionCycleRules.DefaultPositionCycleValidator(),
            clock,
            new List<string>());
    }
}
