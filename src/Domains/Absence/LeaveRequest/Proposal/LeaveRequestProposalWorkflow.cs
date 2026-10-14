namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves leave request proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestProposalDesk(ILeaveRequestProposalStore Store, LeaveRequestProposalValidator Validator, IClock Clock, List<string> Journal, LeaveRequestProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestProposalStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestProposalStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request proposal {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of leave request proposal workflow.</summary>
public static class LeaveRequestProposalWorkflow
{
    public static LeaveRequestProposalDesk NewLeaveRequestProposalDesk(ILeaveRequestProposalStore store, IClock clock, Registry flags)
    {
        return new LeaveRequestProposalDesk(
            store,
            LeaveRequestProposalRules.DefaultLeaveRequestProposalValidator(),
            clock,
            new List<string>(),
            LeaveRequestProposalEngine.DefaultLeaveRequestProposalEngine(flags));
    }
}
