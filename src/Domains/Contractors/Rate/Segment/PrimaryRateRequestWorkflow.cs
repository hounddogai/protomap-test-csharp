namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary rate requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryRateRequestService(IPrimaryRateRequestStore Store, PrimaryRateRequestValidator Validator, IClock Clock, List<string> Journal, PrimaryRateRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary rate request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryRateRequestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary rate request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryRateRequestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary rate request {code} not found");
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

/// <summary>Functions of primary rate request workflow.</summary>
public static class PrimaryRateRequestWorkflow
{
    public static PrimaryRateRequestService NewPrimaryRateRequestService(IPrimaryRateRequestStore store, IClock clock)
    {
        return new PrimaryRateRequestService(
            store,
            PrimaryRateRequestRules.DefaultPrimaryRateRequestValidator(),
            clock,
            new List<string>(),
            PrimaryRateRequestEngine.DefaultPrimaryRateRequestEngine());
    }
}
