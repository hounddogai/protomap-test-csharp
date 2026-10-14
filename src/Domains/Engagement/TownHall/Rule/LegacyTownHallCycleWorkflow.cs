namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy town hall cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyTownHallCycleDesk(ILegacyTownHallCycleStore Store, LegacyTownHallCycleValidator Validator, IClock Clock, List<string> Journal, LegacyTownHallCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy town hall cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyTownHallCycleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy town hall cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyTownHallCycleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy town hall cycle {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of legacy town hall cycle workflow.</summary>
public static class LegacyTownHallCycleWorkflow
{
    public static LegacyTownHallCycleDesk NewLegacyTownHallCycleDesk(ILegacyTownHallCycleStore store, IClock clock, Registry flags)
    {
        return new LegacyTownHallCycleDesk(
            store,
            LegacyTownHallCycleRules.DefaultLegacyTownHallCycleValidator(),
            clock,
            new List<string>(),
            LegacyTownHallCycleEngine.DefaultLegacyTownHallCycleEngine(flags));
    }
}
