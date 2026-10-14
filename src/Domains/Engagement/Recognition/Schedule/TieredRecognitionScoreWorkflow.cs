namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered recognition scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredRecognitionScoreService(ITieredRecognitionScoreStore Store, TieredRecognitionScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered recognition score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredRecognitionScoreStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered recognition score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredRecognitionScoreStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of tiered recognition score workflow.</summary>
public static class TieredRecognitionScoreWorkflow
{
    public static TieredRecognitionScoreService NewTieredRecognitionScoreService(ITieredRecognitionScoreStore store, IClock clock)
    {
        return new TieredRecognitionScoreService(
            store,
            TieredRecognitionScoreRules.DefaultTieredRecognitionScoreValidator(),
            clock,
            new List<string>());
    }
}
