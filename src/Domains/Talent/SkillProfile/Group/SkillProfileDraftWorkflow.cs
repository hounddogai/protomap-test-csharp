namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves skill profile drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileDraftCoordinator(ISkillProfileDraftStore Store, SkillProfileDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileDraftStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileDraftStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of skill profile draft workflow.</summary>
public static class SkillProfileDraftWorkflow
{
    public static SkillProfileDraftCoordinator NewSkillProfileDraftCoordinator(ISkillProfileDraftStore store, IClock clock)
    {
        return new SkillProfileDraftCoordinator(
            store,
            SkillProfileDraftRules.DefaultSkillProfileDraftValidator(),
            clock,
            new List<string>());
    }
}
