namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending scenario templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingScenarioTemplateDesk(IPendingScenarioTemplateStore Store, PendingScenarioTemplateValidator Validator, IClock Clock, List<string> Journal, PendingScenarioTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending scenario template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingScenarioTemplateStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending scenario template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingScenarioTemplateStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending scenario template {code} not found");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of pending scenario template workflow.</summary>
public static class PendingScenarioTemplateWorkflow
{
    public static PendingScenarioTemplateDesk NewPendingScenarioTemplateDesk(IPendingScenarioTemplateStore store, IClock clock)
    {
        return new PendingScenarioTemplateDesk(
            store,
            PendingScenarioTemplateRules.DefaultPendingScenarioTemplateValidator(),
            clock,
            new List<string>(),
            PendingScenarioTemplateEngine.DefaultPendingScenarioTemplateEngine());
    }
}
