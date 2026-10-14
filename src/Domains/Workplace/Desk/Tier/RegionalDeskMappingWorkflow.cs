namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional desk mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalDeskMappingCoordinator(IRegionalDeskMappingStore Store, RegionalDeskMappingValidator Validator, IClock Clock, List<string> Journal, RegionalDeskMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional desk mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalDeskMappingStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional desk mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalDeskMappingStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional desk mapping {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of regional desk mapping workflow.</summary>
public static class RegionalDeskMappingWorkflow
{
    public static RegionalDeskMappingCoordinator NewRegionalDeskMappingCoordinator(IRegionalDeskMappingStore store, IClock clock, Registry flags)
    {
        return new RegionalDeskMappingCoordinator(
            store,
            RegionalDeskMappingRules.DefaultRegionalDeskMappingValidator(),
            clock,
            new List<string>(),
            RegionalDeskMappingEngine.DefaultRegionalDeskMappingEngine(flags));
    }
}
