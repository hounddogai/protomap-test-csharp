namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary refresh scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryRefreshScoreDesk(IPrimaryRefreshScoreStore Store, PrimaryRefreshScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary refresh score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryRefreshScoreStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary refresh score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryRefreshScoreStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of primary refresh score workflow.</summary>
public static class PrimaryRefreshScoreWorkflow
{
    public static PrimaryRefreshScoreDesk NewPrimaryRefreshScoreDesk(IPrimaryRefreshScoreStore store, IClock clock)
    {
        return new PrimaryRefreshScoreDesk(
            store,
            PrimaryRefreshScoreRules.DefaultPrimaryRefreshScoreValidator(),
            clock,
            new List<string>());
    }
}
