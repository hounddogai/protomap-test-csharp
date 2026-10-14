namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled pulse survey adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledPulseSurveyAdjustmentCoordinator(IScheduledPulseSurveyAdjustmentStore Store, ScheduledPulseSurveyAdjustmentValidator Validator, IClock Clock, List<string> Journal, ScheduledPulseSurveyAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pulse survey adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPulseSurveyAdjustmentStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pulse survey adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPulseSurveyAdjustmentStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pulse survey adjustment {code} not found");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of scheduled pulse survey adjustment workflow.</summary>
public static class ScheduledPulseSurveyAdjustmentWorkflow
{
    public static ScheduledPulseSurveyAdjustmentCoordinator NewScheduledPulseSurveyAdjustmentCoordinator(IScheduledPulseSurveyAdjustmentStore store, IClock clock)
    {
        return new ScheduledPulseSurveyAdjustmentCoordinator(
            store,
            ScheduledPulseSurveyAdjustmentRules.DefaultScheduledPulseSurveyAdjustmentValidator(),
            clock,
            new List<string>(),
            ScheduledPulseSurveyAdjustmentEngine.DefaultScheduledPulseSurveyAdjustmentEngine());
    }
}
