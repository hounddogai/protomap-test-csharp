namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient policy ack cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientPolicyAckCycleCoordinator(ILenientPolicyAckCycleStore Store, LenientPolicyAckCycleValidator Validator, IClock Clock, List<string> Journal, LenientPolicyAckCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient policy ack cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientPolicyAckCycleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient policy ack cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientPolicyAckCycleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient policy ack cycle {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of lenient policy ack cycle workflow.</summary>
public static class LenientPolicyAckCycleWorkflow
{
    public static LenientPolicyAckCycleCoordinator NewLenientPolicyAckCycleCoordinator(ILenientPolicyAckCycleStore store, IClock clock, Registry flags)
    {
        return new LenientPolicyAckCycleCoordinator(
            store,
            LenientPolicyAckCycleRules.DefaultLenientPolicyAckCycleValidator(),
            clock,
            new List<string>(),
            LenientPolicyAckCycleEngine.DefaultLenientPolicyAckCycleEngine(flags));
    }
}
