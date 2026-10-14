namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves certification forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationForecastManager(ICertificationForecastStore Store, CertificationForecastValidator Validator, IClock Clock, List<string> Journal, CertificationForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationForecastStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationForecastStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification forecast {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of certification forecast workflow.</summary>
public static class CertificationForecastWorkflow
{
    public static CertificationForecastManager NewCertificationForecastManager(ICertificationForecastStore store, IClock clock)
    {
        return new CertificationForecastManager(
            store,
            CertificationForecastRules.DefaultCertificationForecastValidator(),
            clock,
            new List<string>(),
            CertificationForecastEngine.DefaultCertificationForecastEngine());
    }
}
