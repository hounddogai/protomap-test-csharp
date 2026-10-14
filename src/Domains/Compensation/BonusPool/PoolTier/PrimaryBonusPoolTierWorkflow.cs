namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary bonus pool tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryBonusPoolTierWorkflow(IPrimaryBonusPoolTierStore Store, PrimaryBonusPoolTierValidator Validator, IClock Clock, List<string> Journal, PrimaryBonusPoolTierEngine Engine)
{
    public static PrimaryBonusPoolTierWorkflow NewPrimaryBonusPoolTierWorkflow(IPrimaryBonusPoolTierStore store, IClock clock)
    {
        return new PrimaryBonusPoolTierWorkflow(
            store,
            PrimaryBonusPoolTierRules.DefaultPrimaryBonusPoolTierValidator(),
            clock,
            new List<string>(),
            PrimaryBonusPoolTierEngine.DefaultPrimaryBonusPoolTierEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary bonus pool tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryBonusPoolTierStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary bonus pool tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryBonusPoolTierStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary bonus pool tier {code} not found");
        }
        var result = Engine.Evaluate(item);
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
