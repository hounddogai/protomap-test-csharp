namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves equity grant requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantRequestDesk(IEquityGrantRequestStore Store, EquityGrantRequestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantRequestStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantRequestStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of equity grant request workflow.</summary>
public static class EquityGrantRequestWorkflow
{
    public static EquityGrantRequestDesk NewEquityGrantRequestDesk(IEquityGrantRequestStore store, IClock clock)
    {
        return new EquityGrantRequestDesk(
            store,
            EquityGrantRequestRules.DefaultEquityGrantRequestValidator(),
            clock,
            new List<string>());
    }
}
