namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves skill profile tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileTierService(ISkillProfileTierStore Store, SkillProfileTierValidator Validator, IClock Clock, List<string> Journal, SkillProfileTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileTierStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileTierStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile tier {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of skill profile tier workflow.</summary>
public static class SkillProfileTierWorkflow
{
    public static SkillProfileTierService NewSkillProfileTierService(ISkillProfileTierStore store, IClock clock)
    {
        return new SkillProfileTierService(
            store,
            SkillProfileTierRules.DefaultSkillProfileTierValidator(),
            clock,
            new List<string>(),
            SkillProfileTierEngine.DefaultSkillProfileTierEngine());
    }
}
