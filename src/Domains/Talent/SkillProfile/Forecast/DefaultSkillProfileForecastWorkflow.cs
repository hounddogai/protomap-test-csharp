namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default skill profile forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultSkillProfileForecastDesk(IDefaultSkillProfileForecastStore Store, DefaultSkillProfileForecastValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default skill profile forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultSkillProfileForecastStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default skill profile forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultSkillProfileForecastStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of default skill profile forecast workflow.</summary>
public static class DefaultSkillProfileForecastWorkflow
{
    public static DefaultSkillProfileForecastDesk NewDefaultSkillProfileForecastDesk(IDefaultSkillProfileForecastStore store, IClock clock)
    {
        return new DefaultSkillProfileForecastDesk(
            store,
            DefaultSkillProfileForecastRules.DefaultDefaultSkillProfileForecastValidator(),
            clock,
            new List<string>());
    }
}
