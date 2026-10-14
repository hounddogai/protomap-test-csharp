namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves policy assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAssignmentManager(IPolicyAssignmentStore Store, PolicyAssignmentValidator Validator, IClock Clock, List<string> Journal, PolicyAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAssignmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAssignmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy assignment {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of policy assignment workflow.</summary>
public static class PolicyAssignmentWorkflow
{
    public static PolicyAssignmentManager NewPolicyAssignmentManager(IPolicyAssignmentStore store, IClock clock, Registry flags)
    {
        return new PolicyAssignmentManager(
            store,
            PolicyAssignmentRules.DefaultPolicyAssignmentValidator(),
            clock,
            new List<string>(),
            PolicyAssignmentEngine.DefaultPolicyAssignmentEngine(flags));
    }
}
