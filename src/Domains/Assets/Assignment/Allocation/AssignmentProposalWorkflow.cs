namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves assignment proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentProposalService(IAssignmentProposalStore Store, AssignmentProposalValidator Validator, IClock Clock, List<string> Journal, AssignmentProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentProposalStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentProposalStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment proposal {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of assignment proposal workflow.</summary>
public static class AssignmentProposalWorkflow
{
    public static AssignmentProposalService NewAssignmentProposalService(IAssignmentProposalStore store, IClock clock, Registry flags)
    {
        return new AssignmentProposalService(
            store,
            AssignmentProposalRules.DefaultAssignmentProposalValidator(),
            clock,
            new List<string>(),
            AssignmentProposalEngine.DefaultAssignmentProposalEngine(flags));
    }
}
