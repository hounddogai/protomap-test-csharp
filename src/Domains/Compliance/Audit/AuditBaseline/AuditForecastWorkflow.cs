namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves audit forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditForecastService(IAuditForecastStore Store, AuditForecastValidator Validator, IClock Clock, List<string> Journal, AuditForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditForecastStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditForecastStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit forecast {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of audit forecast workflow.</summary>
public static class AuditForecastWorkflow
{
    public static AuditForecastService NewAuditForecastService(IAuditForecastStore store, IClock clock, Registry flags)
    {
        return new AuditForecastService(
            store,
            AuditForecastRules.DefaultAuditForecastValidator(),
            clock,
            new List<string>(),
            AuditForecastEngine.DefaultAuditForecastEngine(flags));
    }
}
