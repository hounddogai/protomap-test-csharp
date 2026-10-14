namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted cost model policies through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedCostModelPolicyManager(IWeightedCostModelPolicyStore Store, WeightedCostModelPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted cost model policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedCostModelPolicyStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted cost model policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedCostModelPolicyStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of weighted cost model policy workflow.</summary>
public static class WeightedCostModelPolicyWorkflow
{
    public static WeightedCostModelPolicyManager NewWeightedCostModelPolicyManager(IWeightedCostModelPolicyStore store, IClock clock)
    {
        return new WeightedCostModelPolicyManager(
            store,
            WeightedCostModelPolicyRules.DefaultWeightedCostModelPolicyValidator(),
            clock,
            new List<string>());
    }
}
