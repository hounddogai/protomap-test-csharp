namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyAdjustmentManager(IPulseSurveyAdjustmentStore Store, PulseSurveyAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyAdjustmentStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyAdjustmentStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of pulse survey adjustment workflow.</summary>
public static class PulseSurveyAdjustmentWorkflow
{
    public static PulseSurveyAdjustmentManager NewPulseSurveyAdjustmentManager(IPulseSurveyAdjustmentStore store, IClock clock)
    {
        return new PulseSurveyAdjustmentManager(
            store,
            PulseSurveyAdjustmentRules.DefaultPulseSurveyAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
