namespace Acme.Workforce.Assets.Assignment.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled assignment claims through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledAssignmentClaimWorkflow(IScheduledAssignmentClaimStore Store, ScheduledAssignmentClaimValidator Validator, IClock Clock, List<string> Journal, ScheduledAssignmentClaimEngine Engine)
{
    public static ScheduledAssignmentClaimWorkflow NewScheduledAssignmentClaimWorkflow(IScheduledAssignmentClaimStore store, IClock clock, Registry flags)
    {
        return new ScheduledAssignmentClaimWorkflow(
            store,
            ScheduledAssignmentClaimRules.DefaultScheduledAssignmentClaimValidator(),
            clock,
            new List<string>(),
            ScheduledAssignmentClaimEngine.DefaultScheduledAssignmentClaimEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled assignment claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledAssignmentClaimStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled assignment claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledAssignmentClaimStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled assignment claim {code} not found");
        }
        var result = Engine.Run(item);
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
