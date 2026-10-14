namespace Acme.Workforce.Engagement.TownHall.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional town hall groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalTownHallGroupWorkflow(IRegionalTownHallGroupStore Store, RegionalTownHallGroupValidator Validator, IClock Clock, List<string> Journal, RegionalTownHallGroupEngine Engine)
{
    public static RegionalTownHallGroupWorkflow NewRegionalTownHallGroupWorkflow(IRegionalTownHallGroupStore store, IClock clock)
    {
        return new RegionalTownHallGroupWorkflow(
            store,
            RegionalTownHallGroupRules.DefaultRegionalTownHallGroupValidator(),
            clock,
            new List<string>(),
            RegionalTownHallGroupEngine.DefaultRegionalTownHallGroupEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional town hall group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalTownHallGroupStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional town hall group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalTownHallGroupStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional town hall group {code} not found");
        }
        var result = Engine.Compute(item);
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
