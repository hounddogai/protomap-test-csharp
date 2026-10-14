namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves succession plan mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record SuccessionPlanMappingManager(ISuccessionPlanMappingStore Store, SuccessionPlanMappingValidator Validator, IClock Clock, List<string> Journal, SuccessionPlanMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanMappingStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanMappingStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan mapping {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of succession plan mapping workflow.</summary>
public static class SuccessionPlanMappingWorkflow
{
    public static SuccessionPlanMappingManager NewSuccessionPlanMappingManager(ISuccessionPlanMappingStore store, IClock clock)
    {
        return new SuccessionPlanMappingManager(
            store,
            SuccessionPlanMappingRules.DefaultSuccessionPlanMappingValidator(),
            clock,
            new List<string>(),
            SuccessionPlanMappingEngine.DefaultSuccessionPlanMappingEngine());
    }
}
