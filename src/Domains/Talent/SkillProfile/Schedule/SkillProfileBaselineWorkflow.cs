namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves skill profile baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileBaselineManager(ISkillProfileBaselineStore Store, SkillProfileBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileBaselineStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileBaselineStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of skill profile baseline workflow.</summary>
public static class SkillProfileBaselineWorkflow
{
    public static SkillProfileBaselineManager NewSkillProfileBaselineManager(ISkillProfileBaselineStore store, IClock clock)
    {
        return new SkillProfileBaselineManager(
            store,
            SkillProfileBaselineRules.DefaultSkillProfileBaselineValidator(),
            clock,
            new List<string>());
    }
}
