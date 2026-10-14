namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient offboarding forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientOffboardingForecastWorkflow(ILenientOffboardingForecastStore Store, LenientOffboardingForecastValidator Validator, IClock Clock, List<string> Journal, LenientOffboardingForecastEngine Engine)
{
    public static LenientOffboardingForecastWorkflow NewLenientOffboardingForecastWorkflow(ILenientOffboardingForecastStore store, IClock clock)
    {
        return new LenientOffboardingForecastWorkflow(
            store,
            LenientOffboardingForecastRules.DefaultLenientOffboardingForecastValidator(),
            clock,
            new List<string>(),
            LenientOffboardingForecastEngine.DefaultLenientOffboardingForecastEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient offboarding forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientOffboardingForecastStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient offboarding forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientOffboardingForecastStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient offboarding forecast {code} not found");
        }
        var result = Engine.Price(item);
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
