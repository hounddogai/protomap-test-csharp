namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional skill profile records through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalSkillProfileRecordCoordinator(IRegionalSkillProfileRecordStore Store, RegionalSkillProfileRecordValidator Validator, IClock Clock, List<string> Journal, RegionalSkillProfileRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional skill profile record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalSkillProfileRecordStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional skill profile record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalSkillProfileRecordStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional skill profile record {code} not found");
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

/// <summary>Functions of regional skill profile record workflow.</summary>
public static class RegionalSkillProfileRecordWorkflow
{
    public static RegionalSkillProfileRecordCoordinator NewRegionalSkillProfileRecordCoordinator(IRegionalSkillProfileRecordStore store, IClock clock, Registry flags)
    {
        return new RegionalSkillProfileRecordCoordinator(
            store,
            RegionalSkillProfileRecordRules.DefaultRegionalSkillProfileRecordValidator(),
            clock,
            new List<string>(),
            RegionalSkillProfileRecordEngine.DefaultRegionalSkillProfileRecordEngine(flags));
    }
}
