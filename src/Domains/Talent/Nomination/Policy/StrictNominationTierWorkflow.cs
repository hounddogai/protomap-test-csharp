namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict nomination tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictNominationTierWorkflow(IStrictNominationTierStore Store, StrictNominationTierValidator Validator, IClock Clock, List<string> Journal, StrictNominationTierEngine Engine)
{
    public static StrictNominationTierWorkflow NewStrictNominationTierWorkflow(IStrictNominationTierStore store, IClock clock)
    {
        return new StrictNominationTierWorkflow(
            store,
            StrictNominationTierRules.DefaultStrictNominationTierValidator(),
            clock,
            new List<string>(),
            StrictNominationTierEngine.DefaultStrictNominationTierEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict nomination tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictNominationTierStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict nomination tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictNominationTierStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict nomination tier {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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
