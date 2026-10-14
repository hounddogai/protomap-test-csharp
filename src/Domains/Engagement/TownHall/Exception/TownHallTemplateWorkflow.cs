namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallTemplateCoordinator(ITownHallTemplateStore Store, TownHallTemplateValidator Validator, IClock Clock, List<string> Journal, TownHallTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallTemplateStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallTemplateStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall template {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of town hall template workflow.</summary>
public static class TownHallTemplateWorkflow
{
    public static TownHallTemplateCoordinator NewTownHallTemplateCoordinator(ITownHallTemplateStore store, IClock clock)
    {
        return new TownHallTemplateCoordinator(
            store,
            TownHallTemplateRules.DefaultTownHallTemplateValidator(),
            clock,
            new List<string>(),
            TownHallTemplateEngine.DefaultTownHallTemplateEngine());
    }
}
