namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered assignment claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredAssignmentClaimWorkflow(ITieredAssignmentClaimStore Store, TieredAssignmentClaimValidator Validator, IClock Clock, List<string> Journal, TieredAssignmentClaimEngine Engine)
{
    public static TieredAssignmentClaimWorkflow NewTieredAssignmentClaimWorkflow(ITieredAssignmentClaimStore store, IClock clock)
    {
        return new TieredAssignmentClaimWorkflow(
            store,
            TieredAssignmentClaimRules.DefaultTieredAssignmentClaimValidator(),
            clock,
            new List<string>(),
            TieredAssignmentClaimEngine.DefaultTieredAssignmentClaimEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered assignment claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredAssignmentClaimStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered assignment claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredAssignmentClaimStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered assignment claim {code} not found");
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
