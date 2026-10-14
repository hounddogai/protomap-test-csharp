namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional mobility reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalMobilityReviewCoordinator(IRegionalMobilityReviewStore Store, RegionalMobilityReviewValidator Validator, IClock Clock, List<string> Journal, RegionalMobilityReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional mobility review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalMobilityReviewStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional mobility review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalMobilityReviewStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional mobility review {code} not found");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of regional mobility review workflow.</summary>
public static class RegionalMobilityReviewWorkflow
{
    public static RegionalMobilityReviewCoordinator NewRegionalMobilityReviewCoordinator(IRegionalMobilityReviewStore store, IClock clock, Registry flags)
    {
        return new RegionalMobilityReviewCoordinator(
            store,
            RegionalMobilityReviewRules.DefaultRegionalMobilityReviewValidator(),
            clock,
            new List<string>(),
            RegionalMobilityReviewEngine.DefaultRegionalMobilityReviewEngine(flags));
    }
}
