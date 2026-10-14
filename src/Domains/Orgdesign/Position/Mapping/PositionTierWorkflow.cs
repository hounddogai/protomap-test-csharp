namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionTierDesk(IPositionTierStore Store, PositionTierValidator Validator, IClock Clock, List<string> Journal, PositionTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionTierStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionTierStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position tier {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of position tier workflow.</summary>
public static class PositionTierWorkflow
{
    public static PositionTierDesk NewPositionTierDesk(IPositionTierStore store, IClock clock)
    {
        return new PositionTierDesk(
            store,
            PositionTierRules.DefaultPositionTierValidator(),
            clock,
            new List<string>(),
            PositionTierEngine.DefaultPositionTierEngine());
    }
}
