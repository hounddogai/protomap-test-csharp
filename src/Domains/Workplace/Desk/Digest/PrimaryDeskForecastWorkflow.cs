namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary desk forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryDeskForecastService(IPrimaryDeskForecastStore Store, PrimaryDeskForecastValidator Validator, IClock Clock, List<string> Journal, PrimaryDeskForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary desk forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryDeskForecastStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary desk forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryDeskForecastStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary desk forecast {code} not found");
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

/// <summary>Functions of primary desk forecast workflow.</summary>
public static class PrimaryDeskForecastWorkflow
{
    public static PrimaryDeskForecastService NewPrimaryDeskForecastService(IPrimaryDeskForecastStore store, IClock clock, Registry flags)
    {
        return new PrimaryDeskForecastService(
            store,
            PrimaryDeskForecastRules.DefaultPrimaryDeskForecastValidator(),
            clock,
            new List<string>(),
            PrimaryDeskForecastEngine.DefaultPrimaryDeskForecastEngine(flags));
    }
}
