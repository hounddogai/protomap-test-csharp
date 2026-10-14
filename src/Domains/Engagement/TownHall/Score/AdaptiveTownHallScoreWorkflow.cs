namespace Acme.Workforce.Engagement.TownHall.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive town hall scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveTownHallScoreWorkflow(IAdaptiveTownHallScoreStore Store, AdaptiveTownHallScoreValidator Validator, IClock Clock, List<string> Journal, AdaptiveTownHallScoreEngine Engine)
{
    public static AdaptiveTownHallScoreWorkflow NewAdaptiveTownHallScoreWorkflow(IAdaptiveTownHallScoreStore store, IClock clock)
    {
        return new AdaptiveTownHallScoreWorkflow(
            store,
            AdaptiveTownHallScoreRules.DefaultAdaptiveTownHallScoreValidator(),
            clock,
            new List<string>(),
            AdaptiveTownHallScoreEngine.DefaultAdaptiveTownHallScoreEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive town hall score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveTownHallScoreStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive town hall score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveTownHallScoreStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive town hall score {code} not found");
        }
        var result = Engine.Evaluate(item);
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
