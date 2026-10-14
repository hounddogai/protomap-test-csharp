namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallEstimateWorkflow(ITownHallEstimateStore Store, TownHallEstimateValidator Validator, IClock Clock, List<string> Journal, TownHallEstimateEngine Engine)
{
    public static TownHallEstimateWorkflow NewTownHallEstimateWorkflow(ITownHallEstimateStore store, IClock clock)
    {
        return new TownHallEstimateWorkflow(
            store,
            TownHallEstimateRules.DefaultTownHallEstimateValidator(),
            clock,
            new List<string>(),
            TownHallEstimateEngine.DefaultTownHallEstimateEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallEstimateStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallEstimateStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall estimate {code} not found");
        }
        var result = Engine.Price(item);
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
