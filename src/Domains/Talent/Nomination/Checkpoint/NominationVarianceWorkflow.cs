namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves nomination variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationVarianceCoordinator(INominationVarianceStore Store, NominationVarianceValidator Validator, IClock Clock, List<string> Journal, NominationVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationVarianceStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationVarianceStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination variance {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of nomination variance workflow.</summary>
public static class NominationVarianceWorkflow
{
    public static NominationVarianceCoordinator NewNominationVarianceCoordinator(INominationVarianceStore store, IClock clock)
    {
        return new NominationVarianceCoordinator(
            store,
            NominationVarianceRules.DefaultNominationVarianceValidator(),
            clock,
            new List<string>(),
            NominationVarianceEngine.DefaultNominationVarianceEngine());
    }
}
