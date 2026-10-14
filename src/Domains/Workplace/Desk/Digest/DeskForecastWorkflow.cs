namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves desk forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskForecastService(IDeskForecastStore Store, DeskForecastValidator Validator, IClock Clock, List<string> Journal, DeskForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskForecastStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskForecastStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk forecast {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of desk forecast workflow.</summary>
public static class DeskForecastWorkflow
{
    public static DeskForecastService NewDeskForecastService(IDeskForecastStore store, IClock clock, Registry flags)
    {
        return new DeskForecastService(
            store,
            DeskForecastRules.DefaultDeskForecastValidator(),
            clock,
            new List<string>(),
            DeskForecastEngine.DefaultDeskForecastEngine(flags));
    }
}
