namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves skill profile rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileRollupWorkflow(ISkillProfileRollupStore Store, SkillProfileRollupValidator Validator, IClock Clock, List<string> Journal)
{
    public static SkillProfileRollupWorkflow NewSkillProfileRollupWorkflow(ISkillProfileRollupStore store, IClock clock)
    {
        return new SkillProfileRollupWorkflow(
            store,
            SkillProfileRollupRules.DefaultSkillProfileRollupValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileRollupStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileRollupStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
            if (item.StartsAt > cutoff)
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
