namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict pool rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictPoolRuleService(IStrictPoolRuleStore Store, StrictPoolRuleValidator Validator, IClock Clock, List<string> Journal, StrictPoolRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pool rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictPoolRuleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pool rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictPoolRuleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pool rule {code} not found");
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

/// <summary>Functions of strict pool rule workflow.</summary>
public static class StrictPoolRuleWorkflow
{
    public static StrictPoolRuleService NewStrictPoolRuleService(IStrictPoolRuleStore store, IClock clock)
    {
        return new StrictPoolRuleService(
            store,
            StrictPoolRuleRules.DefaultStrictPoolRuleValidator(),
            clock,
            new List<string>(),
            StrictPoolRuleEngine.DefaultStrictPoolRuleEngine());
    }
}
