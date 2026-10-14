namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted policy ack drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedPolicyAckDraftCoordinator(IWeightedPolicyAckDraftStore Store, WeightedPolicyAckDraftValidator Validator, IClock Clock, List<string> Journal, WeightedPolicyAckDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted policy ack draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedPolicyAckDraftStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted policy ack draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedPolicyAckDraftStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted policy ack draft {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of weighted policy ack draft workflow.</summary>
public static class WeightedPolicyAckDraftWorkflow
{
    public static WeightedPolicyAckDraftCoordinator NewWeightedPolicyAckDraftCoordinator(IWeightedPolicyAckDraftStore store, IClock clock)
    {
        return new WeightedPolicyAckDraftCoordinator(
            store,
            WeightedPolicyAckDraftRules.DefaultWeightedPolicyAckDraftValidator(),
            clock,
            new List<string>(),
            WeightedPolicyAckDraftEngine.DefaultWeightedPolicyAckDraftEngine());
    }
}
