namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient nomination forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientNominationForecastWorkflow(ILenientNominationForecastStore Store, LenientNominationForecastValidator Validator, IClock Clock, List<string> Journal, LenientNominationForecastEngine Engine)
{
    public static LenientNominationForecastWorkflow NewLenientNominationForecastWorkflow(ILenientNominationForecastStore store, IClock clock)
    {
        return new LenientNominationForecastWorkflow(
            store,
            LenientNominationForecastRules.DefaultLenientNominationForecastValidator(),
            clock,
            new List<string>(),
            LenientNominationForecastEngine.DefaultLenientNominationForecastEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient nomination forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientNominationForecastStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient nomination forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientNominationForecastStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient nomination forecast {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
