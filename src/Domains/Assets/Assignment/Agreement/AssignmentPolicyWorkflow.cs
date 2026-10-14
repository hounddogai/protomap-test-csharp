namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves assignment policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentPolicyManager(IAssignmentPolicyStore Store, AssignmentPolicyValidator Validator, IClock Clock, List<string> Journal, AssignmentPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentPolicyStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentPolicyStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment policy {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of assignment policy workflow.</summary>
public static class AssignmentPolicyWorkflow
{
    public static AssignmentPolicyManager NewAssignmentPolicyManager(IAssignmentPolicyStore store, IClock clock, Registry flags)
    {
        return new AssignmentPolicyManager(
            store,
            AssignmentPolicyRules.DefaultAssignmentPolicyValidator(),
            clock,
            new List<string>(),
            AssignmentPolicyEngine.DefaultAssignmentPolicyEngine(flags));
    }
}
