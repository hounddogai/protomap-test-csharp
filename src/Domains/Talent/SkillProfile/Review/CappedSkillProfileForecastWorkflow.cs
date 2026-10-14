namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped skill profile forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedSkillProfileForecastCoordinator(ICappedSkillProfileForecastStore Store, CappedSkillProfileForecastValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped skill profile forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedSkillProfileForecastStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped skill profile forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedSkillProfileForecastStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of capped skill profile forecast workflow.</summary>
public static class CappedSkillProfileForecastWorkflow
{
    public static CappedSkillProfileForecastCoordinator NewCappedSkillProfileForecastCoordinator(ICappedSkillProfileForecastStore store, IClock clock)
    {
        return new CappedSkillProfileForecastCoordinator(
            store,
            CappedSkillProfileForecastRules.DefaultCappedSkillProfileForecastValidator(),
            clock,
            new List<string>());
    }
}
