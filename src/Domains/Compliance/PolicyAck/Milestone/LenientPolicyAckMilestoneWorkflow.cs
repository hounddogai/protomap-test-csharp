namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient policy ack milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientPolicyAckMilestoneService(ILenientPolicyAckMilestoneStore Store, LenientPolicyAckMilestoneValidator Validator, IClock Clock, List<string> Journal, LenientPolicyAckMilestoneEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient policy ack milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientPolicyAckMilestoneStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient policy ack milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientPolicyAckMilestoneStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient policy ack milestone {code} not found");
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

/// <summary>Functions of lenient policy ack milestone workflow.</summary>
public static class LenientPolicyAckMilestoneWorkflow
{
    public static LenientPolicyAckMilestoneService NewLenientPolicyAckMilestoneService(ILenientPolicyAckMilestoneStore store, IClock clock, Registry flags)
    {
        return new LenientPolicyAckMilestoneService(
            store,
            LenientPolicyAckMilestoneRules.DefaultLenientPolicyAckMilestoneValidator(),
            clock,
            new List<string>(),
            LenientPolicyAckMilestoneEngine.DefaultLenientPolicyAckMilestoneEngine(flags));
    }
}
