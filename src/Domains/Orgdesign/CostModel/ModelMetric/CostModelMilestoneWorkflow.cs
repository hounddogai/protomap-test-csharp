namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves cost model milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelMilestoneService(ICostModelMilestoneStore Store, CostModelMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelMilestoneStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelMilestoneStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of cost model milestone workflow.</summary>
public static class CostModelMilestoneWorkflow
{
    public static CostModelMilestoneService NewCostModelMilestoneService(ICostModelMilestoneStore store, IClock clock)
    {
        return new CostModelMilestoneService(
            store,
            CostModelMilestoneRules.DefaultCostModelMilestoneValidator(),
            clock,
            new List<string>());
    }
}
