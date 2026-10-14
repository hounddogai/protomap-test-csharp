namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback scenario overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackScenarioOverrideDesk(IFallbackScenarioOverrideStore Store, FallbackScenarioOverrideValidator Validator, IClock Clock, List<string> Journal, FallbackScenarioOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback scenario override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackScenarioOverrideStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback scenario override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackScenarioOverrideStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback scenario override {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of fallback scenario override workflow.</summary>
public static class FallbackScenarioOverrideWorkflow
{
    public static FallbackScenarioOverrideDesk NewFallbackScenarioOverrideDesk(IFallbackScenarioOverrideStore store, IClock clock)
    {
        return new FallbackScenarioOverrideDesk(
            store,
            FallbackScenarioOverrideRules.DefaultFallbackScenarioOverrideValidator(),
            clock,
            new List<string>(),
            FallbackScenarioOverrideEngine.DefaultFallbackScenarioOverrideEngine());
    }
}
