namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rate windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record RateWindowDesk(IRateWindowStore Store, RateWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RateWindowStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RateWindowStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of rate window workflow.</summary>
public static class RateWindowWorkflow
{
    public static RateWindowDesk NewRateWindowDesk(IRateWindowStore store, IClock clock)
    {
        return new RateWindowDesk(store, RateWindowRules.DefaultRateWindowValidator(), clock, new List<string>());
    }
}
