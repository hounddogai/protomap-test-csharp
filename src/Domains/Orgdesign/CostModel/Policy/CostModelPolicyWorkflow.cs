namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves cost model policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelPolicyCoordinator(ICostModelPolicyStore Store, CostModelPolicyValidator Validator, IClock Clock, List<string> Journal, CostModelPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelPolicyStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelPolicyStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model policy {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of cost model policy workflow.</summary>
public static class CostModelPolicyWorkflow
{
    public static CostModelPolicyCoordinator NewCostModelPolicyCoordinator(ICostModelPolicyStore store, IClock clock, Registry flags)
    {
        return new CostModelPolicyCoordinator(
            store,
            CostModelPolicyRules.DefaultCostModelPolicyValidator(),
            clock,
            new List<string>(),
            CostModelPolicyEngine.DefaultCostModelPolicyEngine(flags));
    }
}
