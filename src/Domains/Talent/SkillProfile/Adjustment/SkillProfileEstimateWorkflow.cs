namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves skill profile estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileEstimateDesk(ISkillProfileEstimateStore Store, SkillProfileEstimateValidator Validator, IClock Clock, List<string> Journal, SkillProfileEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileEstimateStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileEstimateStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile estimate {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of skill profile estimate workflow.</summary>
public static class SkillProfileEstimateWorkflow
{
    public static SkillProfileEstimateDesk NewSkillProfileEstimateDesk(ISkillProfileEstimateStore store, IClock clock, Registry flags)
    {
        return new SkillProfileEstimateDesk(
            store,
            SkillProfileEstimateRules.DefaultSkillProfileEstimateValidator(),
            clock,
            new List<string>(),
            SkillProfileEstimateEngine.DefaultSkillProfileEstimateEngine(flags));
    }
}
