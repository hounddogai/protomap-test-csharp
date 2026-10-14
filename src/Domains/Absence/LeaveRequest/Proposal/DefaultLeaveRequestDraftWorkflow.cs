namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default leave request drafts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultLeaveRequestDraftCoordinator(IDefaultLeaveRequestDraftStore Store, DefaultLeaveRequestDraftValidator Validator, IClock Clock, List<string> Journal, DefaultLeaveRequestDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default leave request draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultLeaveRequestDraftStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default leave request draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultLeaveRequestDraftStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default leave request draft {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of default leave request draft workflow.</summary>
public static class DefaultLeaveRequestDraftWorkflow
{
    public static DefaultLeaveRequestDraftCoordinator NewDefaultLeaveRequestDraftCoordinator(IDefaultLeaveRequestDraftStore store, IClock clock, Registry flags)
    {
        return new DefaultLeaveRequestDraftCoordinator(
            store,
            DefaultLeaveRequestDraftRules.DefaultDefaultLeaveRequestDraftValidator(),
            clock,
            new List<string>(),
            DefaultLeaveRequestDraftEngine.DefaultDefaultLeaveRequestDraftEngine(flags));
    }
}
