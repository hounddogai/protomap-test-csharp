namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal pulse survey quotas through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalPulseSurveyQuotaManager(ISeasonalPulseSurveyQuotaStore Store, SeasonalPulseSurveyQuotaValidator Validator, IClock Clock, List<string> Journal, SeasonalPulseSurveyQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pulse survey quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalPulseSurveyQuotaStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pulse survey quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalPulseSurveyQuotaStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pulse survey quota {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of seasonal pulse survey quota workflow.</summary>
public static class SeasonalPulseSurveyQuotaWorkflow
{
    public static SeasonalPulseSurveyQuotaManager NewSeasonalPulseSurveyQuotaManager(ISeasonalPulseSurveyQuotaStore store, IClock clock, Registry flags)
    {
        return new SeasonalPulseSurveyQuotaManager(
            store,
            SeasonalPulseSurveyQuotaRules.DefaultSeasonalPulseSurveyQuotaValidator(),
            clock,
            new List<string>(),
            SeasonalPulseSurveyQuotaEngine.DefaultSeasonalPulseSurveyQuotaEngine(flags));
    }
}
