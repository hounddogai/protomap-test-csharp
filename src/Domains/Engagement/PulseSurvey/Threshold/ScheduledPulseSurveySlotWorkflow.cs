namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled pulse survey slots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledPulseSurveySlotCoordinator(IScheduledPulseSurveySlotStore Store, ScheduledPulseSurveySlotValidator Validator, IClock Clock, List<string> Journal, ScheduledPulseSurveySlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pulse survey slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPulseSurveySlotStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pulse survey slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPulseSurveySlotStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pulse survey slot {code} not found");
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

/// <summary>Functions of scheduled pulse survey slot workflow.</summary>
public static class ScheduledPulseSurveySlotWorkflow
{
    public static ScheduledPulseSurveySlotCoordinator NewScheduledPulseSurveySlotCoordinator(IScheduledPulseSurveySlotStore store, IClock clock)
    {
        return new ScheduledPulseSurveySlotCoordinator(
            store,
            ScheduledPulseSurveySlotRules.DefaultScheduledPulseSurveySlotValidator(),
            clock,
            new List<string>(),
            ScheduledPulseSurveySlotEngine.DefaultScheduledPulseSurveySlotEngine());
    }
}
