namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global policy ack cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalPolicyAckCycleWorkflow(IGlobalPolicyAckCycleStore Store, GlobalPolicyAckCycleValidator Validator, IClock Clock, List<string> Journal, GlobalPolicyAckCycleEngine Engine)
{
    public static GlobalPolicyAckCycleWorkflow NewGlobalPolicyAckCycleWorkflow(IGlobalPolicyAckCycleStore store, IClock clock, Registry flags)
    {
        return new GlobalPolicyAckCycleWorkflow(
            store,
            GlobalPolicyAckCycleRules.DefaultGlobalPolicyAckCycleValidator(),
            clock,
            new List<string>(),
            GlobalPolicyAckCycleEngine.DefaultGlobalPolicyAckCycleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global policy ack cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalPolicyAckCycleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global policy ack cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalPolicyAckCycleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global policy ack cycle {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.StartsAt > cutoff)
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
