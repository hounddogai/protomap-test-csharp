namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scenario templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioTemplateCoordinator(IScenarioTemplateStore Store, ScenarioTemplateValidator Validator, IClock Clock, List<string> Journal, ScenarioTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioTemplateStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioTemplateStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario template {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of scenario template workflow.</summary>
public static class ScenarioTemplateWorkflow
{
    public static ScenarioTemplateCoordinator NewScenarioTemplateCoordinator(IScenarioTemplateStore store, IClock clock, Registry flags)
    {
        return new ScenarioTemplateCoordinator(
            store,
            ScenarioTemplateRules.DefaultScenarioTemplateValidator(),
            clock,
            new List<string>(),
            ScenarioTemplateEngine.DefaultScenarioTemplateEngine(flags));
    }
}
