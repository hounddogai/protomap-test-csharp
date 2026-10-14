namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyThresholdManager(IPulseSurveyThresholdStore Store, PulseSurveyThresholdValidator Validator, IClock Clock, List<string> Journal, PulseSurveyThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyThresholdStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyThresholdStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey threshold {code} not found");
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

/// <summary>Functions of pulse survey threshold workflow.</summary>
public static class PulseSurveyThresholdWorkflow
{
    public static PulseSurveyThresholdManager NewPulseSurveyThresholdManager(IPulseSurveyThresholdStore store, IClock clock)
    {
        return new PulseSurveyThresholdManager(
            store,
            PulseSurveyThresholdRules.DefaultPulseSurveyThresholdValidator(),
            clock,
            new List<string>(),
            PulseSurveyThresholdEngine.DefaultPulseSurveyThresholdEngine());
    }
}
