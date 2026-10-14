namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal skill profile preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalSkillProfilePreferenceWorkflow(ISeasonalSkillProfilePreferenceStore Store, SeasonalSkillProfilePreferenceValidator Validator, IClock Clock, List<string> Journal, SeasonalSkillProfilePreferenceEngine Engine)
{
    public static SeasonalSkillProfilePreferenceWorkflow NewSeasonalSkillProfilePreferenceWorkflow(ISeasonalSkillProfilePreferenceStore store, IClock clock)
    {
        return new SeasonalSkillProfilePreferenceWorkflow(
            store,
            SeasonalSkillProfilePreferenceRules.DefaultSeasonalSkillProfilePreferenceValidator(),
            clock,
            new List<string>(),
            SeasonalSkillProfilePreferenceEngine.DefaultSeasonalSkillProfilePreferenceEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal skill profile preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalSkillProfilePreferenceStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal skill profile preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalSkillProfilePreferenceStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal skill profile preference {code} not found");
        }
        var result = Engine.Evaluate(item);
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
