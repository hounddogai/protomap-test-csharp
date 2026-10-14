namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scenario summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioSummaryService(IScenarioSummaryStore Store, ScenarioSummaryValidator Validator, IClock Clock, List<string> Journal, ScenarioSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioSummaryStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioSummaryStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario summary {code} not found");
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

/// <summary>Functions of scenario summary workflow.</summary>
public static class ScenarioSummaryWorkflow
{
    public static ScenarioSummaryService NewScenarioSummaryService(IScenarioSummaryStore store, IClock clock, Registry flags)
    {
        return new ScenarioSummaryService(
            store,
            ScenarioSummaryRules.DefaultScenarioSummaryValidator(),
            clock,
            new List<string>(),
            ScenarioSummaryEngine.DefaultScenarioSummaryEngine(flags));
    }
}
