namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped skill profile adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedSkillProfileAdjustmentDesk(ICappedSkillProfileAdjustmentStore Store, CappedSkillProfileAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped skill profile adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedSkillProfileAdjustmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped skill profile adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedSkillProfileAdjustmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of capped skill profile adjustment workflow.</summary>
public static class CappedSkillProfileAdjustmentWorkflow
{
    public static CappedSkillProfileAdjustmentDesk NewCappedSkillProfileAdjustmentDesk(ICappedSkillProfileAdjustmentStore store, IClock clock)
    {
        return new CappedSkillProfileAdjustmentDesk(
            store,
            CappedSkillProfileAdjustmentRules.DefaultCappedSkillProfileAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
