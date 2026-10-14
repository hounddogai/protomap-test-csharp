namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy mobility rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyMobilityRuleManager(ILegacyMobilityRuleStore Store, LegacyMobilityRuleValidator Validator, IClock Clock, List<string> Journal, LegacyMobilityRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy mobility rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyMobilityRuleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy mobility rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyMobilityRuleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy mobility rule {code} not found");
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

/// <summary>Functions of legacy mobility rule workflow.</summary>
public static class LegacyMobilityRuleWorkflow
{
    public static LegacyMobilityRuleManager NewLegacyMobilityRuleManager(ILegacyMobilityRuleStore store, IClock clock, Registry flags)
    {
        return new LegacyMobilityRuleManager(
            store,
            LegacyMobilityRuleRules.DefaultLegacyMobilityRuleValidator(),
            clock,
            new List<string>(),
            LegacyMobilityRuleEngine.DefaultLegacyMobilityRuleEngine(flags));
    }
}
