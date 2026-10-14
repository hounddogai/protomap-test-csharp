namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pulse survey forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyForecastDesk(IPulseSurveyForecastStore Store, PulseSurveyForecastValidator Validator, IClock Clock, List<string> Journal, PulseSurveyForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyForecastStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyForecastStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey forecast {code} not found");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of pulse survey forecast workflow.</summary>
public static class PulseSurveyForecastWorkflow
{
    public static PulseSurveyForecastDesk NewPulseSurveyForecastDesk(IPulseSurveyForecastStore store, IClock clock, Registry flags)
    {
        return new PulseSurveyForecastDesk(
            store,
            PulseSurveyForecastRules.DefaultPulseSurveyForecastValidator(),
            clock,
            new List<string>(),
            PulseSurveyForecastEngine.DefaultPulseSurveyForecastEngine(flags));
    }
}
