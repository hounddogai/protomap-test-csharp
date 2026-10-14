namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling pulse survey segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingPulseSurveySegmentWorkflow(IRollingPulseSurveySegmentStore Store, RollingPulseSurveySegmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static RollingPulseSurveySegmentWorkflow NewRollingPulseSurveySegmentWorkflow(IRollingPulseSurveySegmentStore store, IClock clock)
    {
        return new RollingPulseSurveySegmentWorkflow(
            store,
            RollingPulseSurveySegmentRules.DefaultRollingPulseSurveySegmentValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling pulse survey segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingPulseSurveySegmentStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling pulse survey segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingPulseSurveySegmentStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
