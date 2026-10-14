namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict skill profile entries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictSkillProfileEntryService(IStrictSkillProfileEntryStore Store, StrictSkillProfileEntryValidator Validator, IClock Clock, List<string> Journal, StrictSkillProfileEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict skill profile entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictSkillProfileEntryStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict skill profile entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictSkillProfileEntryStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict skill profile entry {code} not found");
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

/// <summary>Functions of strict skill profile entry workflow.</summary>
public static class StrictSkillProfileEntryWorkflow
{
    public static StrictSkillProfileEntryService NewStrictSkillProfileEntryService(IStrictSkillProfileEntryStore store, IClock clock)
    {
        return new StrictSkillProfileEntryService(
            store,
            StrictSkillProfileEntryRules.DefaultStrictSkillProfileEntryValidator(),
            clock,
            new List<string>(),
            StrictSkillProfileEntryEngine.DefaultStrictSkillProfileEntryEngine());
    }
}
