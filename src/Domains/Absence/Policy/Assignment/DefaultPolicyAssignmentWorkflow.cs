namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default policy assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultPolicyAssignmentService(IDefaultPolicyAssignmentStore Store, DefaultPolicyAssignmentValidator Validator, IClock Clock, List<string> Journal, DefaultPolicyAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default policy assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultPolicyAssignmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default policy assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultPolicyAssignmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default policy assignment {code} not found");
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

/// <summary>Functions of default policy assignment workflow.</summary>
public static class DefaultPolicyAssignmentWorkflow
{
    public static DefaultPolicyAssignmentService NewDefaultPolicyAssignmentService(IDefaultPolicyAssignmentStore store, IClock clock, Registry flags)
    {
        return new DefaultPolicyAssignmentService(
            store,
            DefaultPolicyAssignmentRules.DefaultDefaultPolicyAssignmentValidator(),
            clock,
            new List<string>(),
            DefaultPolicyAssignmentEngine.DefaultDefaultPolicyAssignmentEngine(flags));
    }
}
