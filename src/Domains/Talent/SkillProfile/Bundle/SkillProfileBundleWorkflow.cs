namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves skill profile bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileBundleDesk(ISkillProfileBundleStore Store, SkillProfileBundleValidator Validator, IClock Clock, List<string> Journal, SkillProfileBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileBundleStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileBundleStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile bundle {code} not found");
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

/// <summary>Functions of skill profile bundle workflow.</summary>
public static class SkillProfileBundleWorkflow
{
    public static SkillProfileBundleDesk NewSkillProfileBundleDesk(ISkillProfileBundleStore store, IClock clock)
    {
        return new SkillProfileBundleDesk(
            store,
            SkillProfileBundleRules.DefaultSkillProfileBundleValidator(),
            clock,
            new List<string>(),
            SkillProfileBundleEngine.DefaultSkillProfileBundleEngine());
    }
}
