namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pulse survey plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyPlanCoordinator(IPulseSurveyPlanStore Store, PulseSurveyPlanValidator Validator, IClock Clock, List<string> Journal, PulseSurveyPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyPlanStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyPlanStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey plan {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of pulse survey plan workflow.</summary>
public static class PulseSurveyPlanWorkflow
{
    public static PulseSurveyPlanCoordinator NewPulseSurveyPlanCoordinator(IPulseSurveyPlanStore store, IClock clock, Registry flags)
    {
        return new PulseSurveyPlanCoordinator(
            store,
            PulseSurveyPlanRules.DefaultPulseSurveyPlanValidator(),
            clock,
            new List<string>(),
            PulseSurveyPlanEngine.DefaultPulseSurveyPlanEngine(flags));
    }
}
