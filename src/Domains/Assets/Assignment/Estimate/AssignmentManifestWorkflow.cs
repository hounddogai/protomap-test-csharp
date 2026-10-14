namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves assignment manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentManifestCoordinator(IAssignmentManifestStore Store, AssignmentManifestValidator Validator, IClock Clock, List<string> Journal, AssignmentManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentManifestStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentManifestStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment manifest {code} not found");
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

/// <summary>Functions of assignment manifest workflow.</summary>
public static class AssignmentManifestWorkflow
{
    public static AssignmentManifestCoordinator NewAssignmentManifestCoordinator(IAssignmentManifestStore store, IClock clock, Registry flags)
    {
        return new AssignmentManifestCoordinator(
            store,
            AssignmentManifestRules.DefaultAssignmentManifestValidator(),
            clock,
            new List<string>(),
            AssignmentManifestEngine.DefaultAssignmentManifestEngine(flags));
    }
}
