namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling skill profile records through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingSkillProfileRecordService(IRollingSkillProfileRecordStore Store, RollingSkillProfileRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling skill profile record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingSkillProfileRecordStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling skill profile record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingSkillProfileRecordStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of rolling skill profile record workflow.</summary>
public static class RollingSkillProfileRecordWorkflow
{
    public static RollingSkillProfileRecordService NewRollingSkillProfileRecordService(IRollingSkillProfileRecordStore store, IClock clock)
    {
        return new RollingSkillProfileRecordService(
            store,
            RollingSkillProfileRecordRules.DefaultRollingSkillProfileRecordValidator(),
            clock,
            new List<string>());
    }
}
