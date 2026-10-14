namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending cost model policies through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingCostModelPolicyService(IPendingCostModelPolicyStore Store, PendingCostModelPolicyValidator Validator, IClock Clock, List<string> Journal, PendingCostModelPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending cost model policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingCostModelPolicyStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending cost model policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingCostModelPolicyStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending cost model policy {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of pending cost model policy workflow.</summary>
public static class PendingCostModelPolicyWorkflow
{
    public static PendingCostModelPolicyService NewPendingCostModelPolicyService(IPendingCostModelPolicyStore store, IClock clock, Registry flags)
    {
        return new PendingCostModelPolicyService(
            store,
            PendingCostModelPolicyRules.DefaultPendingCostModelPolicyValidator(),
            clock,
            new List<string>(),
            PendingCostModelPolicyEngine.DefaultPendingCostModelPolicyEngine(flags));
    }
}
