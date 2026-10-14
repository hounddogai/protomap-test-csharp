namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary scenario mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryScenarioMappingService(IPrimaryScenarioMappingStore Store, PrimaryScenarioMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary scenario mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryScenarioMappingStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary scenario mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryScenarioMappingStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of primary scenario mapping workflow.</summary>
public static class PrimaryScenarioMappingWorkflow
{
    public static PrimaryScenarioMappingService NewPrimaryScenarioMappingService(IPrimaryScenarioMappingStore store, IClock clock)
    {
        return new PrimaryScenarioMappingService(
            store,
            PrimaryScenarioMappingRules.DefaultPrimaryScenarioMappingValidator(),
            clock,
            new List<string>());
    }
}
