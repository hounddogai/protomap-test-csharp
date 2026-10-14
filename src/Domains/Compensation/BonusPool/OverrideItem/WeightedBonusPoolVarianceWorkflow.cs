namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted bonus pool variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedBonusPoolVarianceDesk(IWeightedBonusPoolVarianceStore Store, WeightedBonusPoolVarianceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted bonus pool variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedBonusPoolVarianceStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted bonus pool variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedBonusPoolVarianceStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of weighted bonus pool variance workflow.</summary>
public static class WeightedBonusPoolVarianceWorkflow
{
    public static WeightedBonusPoolVarianceDesk NewWeightedBonusPoolVarianceDesk(IWeightedBonusPoolVarianceStore store, IClock clock)
    {
        return new WeightedBonusPoolVarianceDesk(
            store,
            WeightedBonusPoolVarianceRules.DefaultWeightedBonusPoolVarianceValidator(),
            clock,
            new List<string>());
    }
}
