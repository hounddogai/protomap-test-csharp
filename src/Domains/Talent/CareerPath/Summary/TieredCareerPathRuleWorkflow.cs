namespace Acme.Workforce.Talent.CareerPath.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered career path rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredCareerPathRuleService(ITieredCareerPathRuleStore Store, TieredCareerPathRuleValidator Validator, IClock Clock, List<string> Journal, TieredCareerPathRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered career path rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredCareerPathRuleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered career path rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredCareerPathRuleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered career path rule {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of tiered career path rule workflow.</summary>
public static class TieredCareerPathRuleWorkflow
{
    public static TieredCareerPathRuleService NewTieredCareerPathRuleService(ITieredCareerPathRuleStore store, IClock clock, Registry flags)
    {
        return new TieredCareerPathRuleService(
            store,
            TieredCareerPathRuleRules.DefaultTieredCareerPathRuleValidator(),
            clock,
            new List<string>(),
            TieredCareerPathRuleEngine.DefaultTieredCareerPathRuleEngine(flags));
    }
}
