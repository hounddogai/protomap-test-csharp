namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy scenario overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyScenarioOverrideCoordinator(ILegacyScenarioOverrideStore Store, LegacyScenarioOverrideValidator Validator, IClock Clock, List<string> Journal, LegacyScenarioOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy scenario override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyScenarioOverrideStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy scenario override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyScenarioOverrideStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy scenario override {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of legacy scenario override workflow.</summary>
public static class LegacyScenarioOverrideWorkflow
{
    public static LegacyScenarioOverrideCoordinator NewLegacyScenarioOverrideCoordinator(ILegacyScenarioOverrideStore store, IClock clock, Registry flags)
    {
        return new LegacyScenarioOverrideCoordinator(
            store,
            LegacyScenarioOverrideRules.DefaultLegacyScenarioOverrideValidator(),
            clock,
            new List<string>(),
            LegacyScenarioOverrideEngine.DefaultLegacyScenarioOverrideEngine(flags));
    }
}
