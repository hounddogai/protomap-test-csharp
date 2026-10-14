namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallScheduleWorkflow(ITownHallScheduleStore Store, TownHallScheduleValidator Validator, IClock Clock, List<string> Journal, TownHallScheduleEngine Engine)
{
    public static TownHallScheduleWorkflow NewTownHallScheduleWorkflow(ITownHallScheduleStore store, IClock clock)
    {
        return new TownHallScheduleWorkflow(
            store,
            TownHallScheduleRules.DefaultTownHallScheduleValidator(),
            clock,
            new List<string>(),
            TownHallScheduleEngine.DefaultTownHallScheduleEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallScheduleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallScheduleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall schedule {code} not found");
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
            if (item.ExpiresAt > cutoff)
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
