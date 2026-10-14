namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted nomination variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedNominationVarianceCoordinator(IWeightedNominationVarianceStore Store, WeightedNominationVarianceValidator Validator, IClock Clock, List<string> Journal, WeightedNominationVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted nomination variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedNominationVarianceStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted nomination variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedNominationVarianceStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted nomination variance {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of weighted nomination variance workflow.</summary>
public static class WeightedNominationVarianceWorkflow
{
    public static WeightedNominationVarianceCoordinator NewWeightedNominationVarianceCoordinator(IWeightedNominationVarianceStore store, IClock clock)
    {
        return new WeightedNominationVarianceCoordinator(
            store,
            WeightedNominationVarianceRules.DefaultWeightedNominationVarianceValidator(),
            clock,
            new List<string>(),
            WeightedNominationVarianceEngine.DefaultWeightedNominationVarianceEngine());
    }
}
