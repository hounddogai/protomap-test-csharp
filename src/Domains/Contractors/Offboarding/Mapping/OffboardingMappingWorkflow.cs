namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves offboarding mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingMappingService(IOffboardingMappingStore Store, OffboardingMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingMappingStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingMappingStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of offboarding mapping workflow.</summary>
public static class OffboardingMappingWorkflow
{
    public static OffboardingMappingService NewOffboardingMappingService(IOffboardingMappingStore store, IClock clock)
    {
        return new OffboardingMappingService(
            store,
            OffboardingMappingRules.DefaultOffboardingMappingValidator(),
            clock,
            new List<string>());
    }
}
