namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending cost model rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingCostModelRuleService(IPendingCostModelRuleStore Store, PendingCostModelRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending cost model rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingCostModelRuleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending cost model rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingCostModelRuleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of pending cost model rule workflow.</summary>
public static class PendingCostModelRuleWorkflow
{
    public static PendingCostModelRuleService NewPendingCostModelRuleService(IPendingCostModelRuleStore store, IClock clock)
    {
        return new PendingCostModelRuleService(
            store,
            PendingCostModelRuleRules.DefaultPendingCostModelRuleValidator(),
            clock,
            new List<string>());
    }
}
