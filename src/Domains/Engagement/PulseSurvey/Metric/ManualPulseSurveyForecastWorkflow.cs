namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual pulse survey forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualPulseSurveyForecastCoordinator(IManualPulseSurveyForecastStore Store, ManualPulseSurveyForecastValidator Validator, IClock Clock, List<string> Journal, ManualPulseSurveyForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualPulseSurveyForecastStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualPulseSurveyForecastStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual pulse survey forecast {code} not found");
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

/// <summary>Functions of manual pulse survey forecast workflow.</summary>
public static class ManualPulseSurveyForecastWorkflow
{
    public static ManualPulseSurveyForecastCoordinator NewManualPulseSurveyForecastCoordinator(IManualPulseSurveyForecastStore store, IClock clock, Registry flags)
    {
        return new ManualPulseSurveyForecastCoordinator(
            store,
            ManualPulseSurveyForecastRules.DefaultManualPulseSurveyForecastValidator(),
            clock,
            new List<string>(),
            ManualPulseSurveyForecastEngine.DefaultManualPulseSurveyForecastEngine(flags));
    }
}
