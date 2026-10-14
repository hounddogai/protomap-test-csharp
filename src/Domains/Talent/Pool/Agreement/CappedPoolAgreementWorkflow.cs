namespace Acme.Workforce.Talent.Pool.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped pool agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedPoolAgreementService(ICappedPoolAgreementStore Store, CappedPoolAgreementValidator Validator, IClock Clock, List<string> Journal, CappedPoolAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped pool agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedPoolAgreementStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped pool agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedPoolAgreementStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped pool agreement {code} not found");
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

/// <summary>Functions of capped pool agreement workflow.</summary>
public static class CappedPoolAgreementWorkflow
{
    public static CappedPoolAgreementService NewCappedPoolAgreementService(ICappedPoolAgreementStore store, IClock clock)
    {
        return new CappedPoolAgreementService(
            store,
            CappedPoolAgreementRules.DefaultCappedPoolAgreementValidator(),
            clock,
            new List<string>(),
            CappedPoolAgreementEngine.DefaultCappedPoolAgreementEngine());
    }
}
