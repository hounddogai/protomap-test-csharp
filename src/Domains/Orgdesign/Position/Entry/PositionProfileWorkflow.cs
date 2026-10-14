namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves position profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionProfileService(IPositionProfileStore Store, PositionProfileValidator Validator, IClock Clock, List<string> Journal, PositionProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionProfileStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionProfileStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position profile {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of position profile workflow.</summary>
public static class PositionProfileWorkflow
{
    public static PositionProfileService NewPositionProfileService(IPositionProfileStore store, IClock clock, Registry flags)
    {
        return new PositionProfileService(
            store,
            PositionProfileRules.DefaultPositionProfileValidator(),
            clock,
            new List<string>(),
            PositionProfileEngine.DefaultPositionProfileEngine(flags));
    }
}
