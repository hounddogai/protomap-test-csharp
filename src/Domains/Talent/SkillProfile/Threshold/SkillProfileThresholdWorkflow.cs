namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves skill profile thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileThresholdCoordinator(ISkillProfileThresholdStore Store, SkillProfileThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileThresholdStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileThresholdStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of skill profile threshold workflow.</summary>
public static class SkillProfileThresholdWorkflow
{
    public static SkillProfileThresholdCoordinator NewSkillProfileThresholdCoordinator(ISkillProfileThresholdStore store, IClock clock)
    {
        return new SkillProfileThresholdCoordinator(
            store,
            SkillProfileThresholdRules.DefaultSkillProfileThresholdValidator(),
            clock,
            new List<string>());
    }
}
