namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves skill profile entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileEntryDesk(ISkillProfileEntryStore Store, SkillProfileEntryValidator Validator, IClock Clock, List<string> Journal, SkillProfileEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileEntryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileEntryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile entry {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of skill profile entry workflow.</summary>
public static class SkillProfileEntryWorkflow
{
    public static SkillProfileEntryDesk NewSkillProfileEntryDesk(ISkillProfileEntryStore store, IClock clock)
    {
        return new SkillProfileEntryDesk(
            store,
            SkillProfileEntryRules.DefaultSkillProfileEntryValidator(),
            clock,
            new List<string>(),
            SkillProfileEntryEngine.DefaultSkillProfileEntryEngine());
    }
}
