namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy town hall scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyTownHallScoreDesk(ILegacyTownHallScoreStore Store, LegacyTownHallScoreValidator Validator, IClock Clock, List<string> Journal, LegacyTownHallScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy town hall score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyTownHallScoreStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy town hall score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyTownHallScoreStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy town hall score {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of legacy town hall score workflow.</summary>
public static class LegacyTownHallScoreWorkflow
{
    public static LegacyTownHallScoreDesk NewLegacyTownHallScoreDesk(ILegacyTownHallScoreStore store, IClock clock, Registry flags)
    {
        return new LegacyTownHallScoreDesk(
            store,
            LegacyTownHallScoreRules.DefaultLegacyTownHallScoreValidator(),
            clock,
            new List<string>(),
            LegacyTownHallScoreEngine.DefaultLegacyTownHallScoreEngine(flags));
    }
}
