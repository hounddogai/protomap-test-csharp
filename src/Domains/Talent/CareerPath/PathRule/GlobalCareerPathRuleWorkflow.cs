namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global career path rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalCareerPathRuleCoordinator(IGlobalCareerPathRuleStore Store, GlobalCareerPathRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global career path rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalCareerPathRuleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global career path rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalCareerPathRuleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of global career path rule workflow.</summary>
public static class GlobalCareerPathRuleWorkflow
{
    public static GlobalCareerPathRuleCoordinator NewGlobalCareerPathRuleCoordinator(IGlobalCareerPathRuleStore store, IClock clock)
    {
        return new GlobalCareerPathRuleCoordinator(
            store,
            GlobalCareerPathRuleRules.DefaultGlobalCareerPathRuleValidator(),
            clock,
            new List<string>());
    }
}
