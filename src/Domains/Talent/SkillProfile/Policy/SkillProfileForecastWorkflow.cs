namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves skill profile forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileForecastCoordinator(ISkillProfileForecastStore Store, SkillProfileForecastValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileForecastStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileForecastStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of skill profile forecast workflow.</summary>
public static class SkillProfileForecastWorkflow
{
    public static SkillProfileForecastCoordinator NewSkillProfileForecastCoordinator(ISkillProfileForecastStore store, IClock clock)
    {
        return new SkillProfileForecastCoordinator(
            store,
            SkillProfileForecastRules.DefaultSkillProfileForecastValidator(),
            clock,
            new List<string>());
    }
}
