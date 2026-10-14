namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves skill profile records through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileRecordDesk(ISkillProfileRecordStore Store, SkillProfileRecordValidator Validator, IClock Clock, List<string> Journal, SkillProfileRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileRecordStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileRecordStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile record {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of skill profile record workflow.</summary>
public static class SkillProfileRecordWorkflow
{
    public static SkillProfileRecordDesk NewSkillProfileRecordDesk(ISkillProfileRecordStore store, IClock clock, Registry flags)
    {
        return new SkillProfileRecordDesk(
            store,
            SkillProfileRecordRules.DefaultSkillProfileRecordValidator(),
            clock,
            new List<string>(),
            SkillProfileRecordEngine.DefaultSkillProfileRecordEngine(flags));
    }
}
