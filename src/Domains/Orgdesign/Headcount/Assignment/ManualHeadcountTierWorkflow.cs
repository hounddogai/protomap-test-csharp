namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual headcount tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualHeadcountTierManager(IManualHeadcountTierStore Store, ManualHeadcountTierValidator Validator, IClock Clock, List<string> Journal, ManualHeadcountTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual headcount tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualHeadcountTierStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual headcount tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualHeadcountTierStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual headcount tier {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of manual headcount tier workflow.</summary>
public static class ManualHeadcountTierWorkflow
{
    public static ManualHeadcountTierManager NewManualHeadcountTierManager(IManualHeadcountTierStore store, IClock clock)
    {
        return new ManualHeadcountTierManager(
            store,
            ManualHeadcountTierRules.DefaultManualHeadcountTierValidator(),
            clock,
            new List<string>(),
            ManualHeadcountTierEngine.DefaultManualHeadcountTierEngine());
    }
}
