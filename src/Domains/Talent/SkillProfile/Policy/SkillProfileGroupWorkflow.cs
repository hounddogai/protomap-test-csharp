namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves skill profile groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileGroupService(ISkillProfileGroupStore Store, SkillProfileGroupValidator Validator, IClock Clock, List<string> Journal, SkillProfileGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileGroupStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileGroupStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile group {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of skill profile group workflow.</summary>
public static class SkillProfileGroupWorkflow
{
    public static SkillProfileGroupService NewSkillProfileGroupService(ISkillProfileGroupStore store, IClock clock, Registry flags)
    {
        return new SkillProfileGroupService(
            store,
            SkillProfileGroupRules.DefaultSkillProfileGroupValidator(),
            clock,
            new List<string>(),
            SkillProfileGroupEngine.DefaultSkillProfileGroupEngine(flags));
    }
}
