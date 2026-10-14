namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default skill profile drafts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultSkillProfileDraftCoordinator(IDefaultSkillProfileDraftStore Store, DefaultSkillProfileDraftValidator Validator, IClock Clock, List<string> Journal, DefaultSkillProfileDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default skill profile draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultSkillProfileDraftStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default skill profile draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultSkillProfileDraftStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default skill profile draft {code} not found");
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

/// <summary>Functions of default skill profile draft workflow.</summary>
public static class DefaultSkillProfileDraftWorkflow
{
    public static DefaultSkillProfileDraftCoordinator NewDefaultSkillProfileDraftCoordinator(IDefaultSkillProfileDraftStore store, IClock clock, Registry flags)
    {
        return new DefaultSkillProfileDraftCoordinator(
            store,
            DefaultSkillProfileDraftRules.DefaultDefaultSkillProfileDraftValidator(),
            clock,
            new List<string>(),
            DefaultSkillProfileDraftEngine.DefaultDefaultSkillProfileDraftEngine(flags));
    }
}
