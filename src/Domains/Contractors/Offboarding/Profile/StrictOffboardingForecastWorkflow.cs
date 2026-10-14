namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict offboarding forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictOffboardingForecastManager(IStrictOffboardingForecastStore Store, StrictOffboardingForecastValidator Validator, IClock Clock, List<string> Journal, StrictOffboardingForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict offboarding forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictOffboardingForecastStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict offboarding forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictOffboardingForecastStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict offboarding forecast {code} not found");
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

/// <summary>Functions of strict offboarding forecast workflow.</summary>
public static class StrictOffboardingForecastWorkflow
{
    public static StrictOffboardingForecastManager NewStrictOffboardingForecastManager(IStrictOffboardingForecastStore store, IClock clock, Registry flags)
    {
        return new StrictOffboardingForecastManager(
            store,
            StrictOffboardingForecastRules.DefaultStrictOffboardingForecastValidator(),
            clock,
            new List<string>(),
            StrictOffboardingForecastEngine.DefaultStrictOffboardingForecastEngine(flags));
    }
}
