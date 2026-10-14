namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyMetricManager(IPulseSurveyMetricStore Store, PulseSurveyMetricValidator Validator, IClock Clock, List<string> Journal, PulseSurveyMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyMetricStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyMetricStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey metric {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of pulse survey metric workflow.</summary>
public static class PulseSurveyMetricWorkflow
{
    public static PulseSurveyMetricManager NewPulseSurveyMetricManager(IPulseSurveyMetricStore store, IClock clock)
    {
        return new PulseSurveyMetricManager(
            store,
            PulseSurveyMetricRules.DefaultPulseSurveyMetricValidator(),
            clock,
            new List<string>(),
            PulseSurveyMetricEngine.DefaultPulseSurveyMetricEngine());
    }
}
