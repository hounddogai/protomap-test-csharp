namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves desk mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskMappingCoordinator(IDeskMappingStore Store, DeskMappingValidator Validator, IClock Clock, List<string> Journal, DeskMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskMappingStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskMappingStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk mapping {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of desk mapping workflow.</summary>
public static class DeskMappingWorkflow
{
    public static DeskMappingCoordinator NewDeskMappingCoordinator(IDeskMappingStore store, IClock clock)
    {
        return new DeskMappingCoordinator(
            store,
            DeskMappingRules.DefaultDeskMappingValidator(),
            clock,
            new List<string>(),
            DeskMappingEngine.DefaultDeskMappingEngine());
    }
}
