namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandMappingManager(IPayBandMappingStore Store, PayBandMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandMappingStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandMappingStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of pay band mapping workflow.</summary>
public static class PayBandMappingWorkflow
{
    public static PayBandMappingManager NewPayBandMappingManager(IPayBandMappingStore store, IClock clock)
    {
        return new PayBandMappingManager(
            store,
            PayBandMappingRules.DefaultPayBandMappingValidator(),
            clock,
            new List<string>());
    }
}
