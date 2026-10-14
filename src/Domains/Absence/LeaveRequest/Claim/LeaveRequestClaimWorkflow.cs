namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves leave request claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestClaimService(ILeaveRequestClaimStore Store, LeaveRequestClaimValidator Validator, IClock Clock, List<string> Journal, LeaveRequestClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestClaimStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestClaimStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request claim {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of leave request claim workflow.</summary>
public static class LeaveRequestClaimWorkflow
{
    public static LeaveRequestClaimService NewLeaveRequestClaimService(ILeaveRequestClaimStore store, IClock clock)
    {
        return new LeaveRequestClaimService(
            store,
            LeaveRequestClaimRules.DefaultLeaveRequestClaimValidator(),
            clock,
            new List<string>(),
            LeaveRequestClaimEngine.DefaultLeaveRequestClaimEngine());
    }
}
