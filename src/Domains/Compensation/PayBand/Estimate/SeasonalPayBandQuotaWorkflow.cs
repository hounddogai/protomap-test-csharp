namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal pay band quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalPayBandQuotaCoordinator(ISeasonalPayBandQuotaStore Store, SeasonalPayBandQuotaValidator Validator, IClock Clock, List<string> Journal, SeasonalPayBandQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pay band quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalPayBandQuotaStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pay band quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalPayBandQuotaStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pay band quota {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of seasonal pay band quota workflow.</summary>
public static class SeasonalPayBandQuotaWorkflow
{
    public static SeasonalPayBandQuotaCoordinator NewSeasonalPayBandQuotaCoordinator(ISeasonalPayBandQuotaStore store, IClock clock)
    {
        return new SeasonalPayBandQuotaCoordinator(
            store,
            SeasonalPayBandQuotaRules.DefaultSeasonalPayBandQuotaValidator(),
            clock,
            new List<string>(),
            SeasonalPayBandQuotaEngine.DefaultSeasonalPayBandQuotaEngine());
    }
}
