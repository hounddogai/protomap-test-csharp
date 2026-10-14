namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves skill profile notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileNoticeDesk(ISkillProfileNoticeStore Store, SkillProfileNoticeValidator Validator, IClock Clock, List<string> Journal, SkillProfileNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileNoticeStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileNoticeStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile notice {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of skill profile notice workflow.</summary>
public static class SkillProfileNoticeWorkflow
{
    public static SkillProfileNoticeDesk NewSkillProfileNoticeDesk(ISkillProfileNoticeStore store, IClock clock, Registry flags)
    {
        return new SkillProfileNoticeDesk(
            store,
            SkillProfileNoticeRules.DefaultSkillProfileNoticeValidator(),
            clock,
            new List<string>(),
            SkillProfileNoticeEngine.DefaultSkillProfileNoticeEngine(flags));
    }
}
