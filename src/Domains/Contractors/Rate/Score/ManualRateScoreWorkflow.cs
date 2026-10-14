namespace Acme.Workforce.Contractors.Rate.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual rate scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualRateScoreCoordinator(IManualRateScoreStore Store, ManualRateScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual rate score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualRateScoreStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual rate score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualRateScoreStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of manual rate score workflow.</summary>
public static class ManualRateScoreWorkflow
{
    public static ManualRateScoreCoordinator NewManualRateScoreCoordinator(IManualRateScoreStore store, IClock clock)
    {
        return new ManualRateScoreCoordinator(
            store,
            ManualRateScoreRules.DefaultManualRateScoreValidator(),
            clock,
            new List<string>());
    }
}
