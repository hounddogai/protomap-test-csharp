namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual pulse survey tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualPulseSurveyTierService(IManualPulseSurveyTierStore Store, ManualPulseSurveyTierValidator Validator, IClock Clock, List<string> Journal, ManualPulseSurveyTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualPulseSurveyTierStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualPulseSurveyTierStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey tier {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of manual pulse survey tier workflow.</summary>
public static class ManualPulseSurveyTierWorkflow
{
    public static ManualPulseSurveyTierService NewManualPulseSurveyTierService(IManualPulseSurveyTierStore store, IClock clock, Registry flags)
    {
        return new ManualPulseSurveyTierService(
            store,
            ManualPulseSurveyTierRules.DefaultManualPulseSurveyTierValidator(),
            clock,
            new List<string>(),
            ManualPulseSurveyTierEngine.DefaultManualPulseSurveyTierEngine(flags));
    }
}
