namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal retro pay reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalRetroPayReviewManager(ISeasonalRetroPayReviewStore Store, SeasonalRetroPayReviewValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal retro pay review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalRetroPayReviewStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal retro pay review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalRetroPayReviewStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of seasonal retro pay review workflow.</summary>
public static class SeasonalRetroPayReviewWorkflow
{
    public static SeasonalRetroPayReviewManager NewSeasonalRetroPayReviewManager(ISeasonalRetroPayReviewStore store, IClock clock)
    {
        return new SeasonalRetroPayReviewManager(
            store,
            SeasonalRetroPayReviewRules.DefaultSeasonalRetroPayReviewValidator(),
            clock,
            new List<string>());
    }
}
