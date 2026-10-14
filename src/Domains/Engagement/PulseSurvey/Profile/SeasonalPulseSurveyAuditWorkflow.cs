namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal pulse survey audits through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalPulseSurveyAuditCoordinator(ISeasonalPulseSurveyAuditStore Store, SeasonalPulseSurveyAuditValidator Validator, IClock Clock, List<string> Journal, SeasonalPulseSurveyAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pulse survey audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalPulseSurveyAuditStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pulse survey audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalPulseSurveyAuditStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pulse survey audit {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of seasonal pulse survey audit workflow.</summary>
public static class SeasonalPulseSurveyAuditWorkflow
{
    public static SeasonalPulseSurveyAuditCoordinator NewSeasonalPulseSurveyAuditCoordinator(ISeasonalPulseSurveyAuditStore store, IClock clock)
    {
        return new SeasonalPulseSurveyAuditCoordinator(
            store,
            SeasonalPulseSurveyAuditRules.DefaultSeasonalPulseSurveyAuditValidator(),
            clock,
            new List<string>(),
            SeasonalPulseSurveyAuditEngine.DefaultSeasonalPulseSurveyAuditEngine());
    }
}
