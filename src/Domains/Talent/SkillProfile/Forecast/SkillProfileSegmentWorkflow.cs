namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves skill profile segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileSegmentManager(ISkillProfileSegmentStore Store, SkillProfileSegmentValidator Validator, IClock Clock, List<string> Journal, SkillProfileSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileSegmentStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileSegmentStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile segment {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of skill profile segment workflow.</summary>
public static class SkillProfileSegmentWorkflow
{
    public static SkillProfileSegmentManager NewSkillProfileSegmentManager(ISkillProfileSegmentStore store, IClock clock, Registry flags)
    {
        return new SkillProfileSegmentManager(
            store,
            SkillProfileSegmentRules.DefaultSkillProfileSegmentValidator(),
            clock,
            new List<string>(),
            SkillProfileSegmentEngine.DefaultSkillProfileSegmentEngine(flags));
    }
}
