namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves succession plan preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SuccessionPlanPreferenceManager(ISuccessionPlanPreferenceStore Store, SuccessionPlanPreferenceValidator Validator, IClock Clock, List<string> Journal, SuccessionPlanPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanPreferenceStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanPreferenceStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan preference {code} not found");
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

/// <summary>Functions of succession plan preference workflow.</summary>
public static class SuccessionPlanPreferenceWorkflow
{
    public static SuccessionPlanPreferenceManager NewSuccessionPlanPreferenceManager(ISuccessionPlanPreferenceStore store, IClock clock, Registry flags)
    {
        return new SuccessionPlanPreferenceManager(
            store,
            SuccessionPlanPreferenceRules.DefaultSuccessionPlanPreferenceValidator(),
            clock,
            new List<string>(),
            SuccessionPlanPreferenceEngine.DefaultSuccessionPlanPreferenceEngine(flags));
    }
}
