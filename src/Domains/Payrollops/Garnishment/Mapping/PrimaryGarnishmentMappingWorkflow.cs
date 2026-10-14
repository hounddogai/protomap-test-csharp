namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary garnishment mappings through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryGarnishmentMappingService(IPrimaryGarnishmentMappingStore Store, PrimaryGarnishmentMappingValidator Validator, IClock Clock, List<string> Journal, PrimaryGarnishmentMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary garnishment mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryGarnishmentMappingStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary garnishment mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryGarnishmentMappingStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary garnishment mapping {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of primary garnishment mapping workflow.</summary>
public static class PrimaryGarnishmentMappingWorkflow
{
    public static PrimaryGarnishmentMappingService NewPrimaryGarnishmentMappingService(IPrimaryGarnishmentMappingStore store, IClock clock)
    {
        return new PrimaryGarnishmentMappingService(
            store,
            PrimaryGarnishmentMappingRules.DefaultPrimaryGarnishmentMappingValidator(),
            clock,
            new List<string>(),
            PrimaryGarnishmentMappingEngine.DefaultPrimaryGarnishmentMappingEngine());
    }
}
