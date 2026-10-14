namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentRequestService(IAdjustmentRequestStore Store, AdjustmentRequestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentRequestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentRequestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of adjustment request workflow.</summary>
public static class AdjustmentRequestWorkflow
{
    public static AdjustmentRequestService NewAdjustmentRequestService(IAdjustmentRequestStore store, IClock clock)
    {
        return new AdjustmentRequestService(
            store,
            AdjustmentRequestRules.DefaultAdjustmentRequestValidator(),
            clock,
            new List<string>());
    }
}
