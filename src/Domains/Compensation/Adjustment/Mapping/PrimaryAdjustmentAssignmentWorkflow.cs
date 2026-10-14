namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary adjustment assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryAdjustmentAssignmentManager(IPrimaryAdjustmentAssignmentStore Store, PrimaryAdjustmentAssignmentValidator Validator, IClock Clock, List<string> Journal, PrimaryAdjustmentAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary adjustment assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryAdjustmentAssignmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary adjustment assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryAdjustmentAssignmentStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary adjustment assignment {code} not found");
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

/// <summary>Functions of primary adjustment assignment workflow.</summary>
public static class PrimaryAdjustmentAssignmentWorkflow
{
    public static PrimaryAdjustmentAssignmentManager NewPrimaryAdjustmentAssignmentManager(IPrimaryAdjustmentAssignmentStore store, IClock clock, Registry flags)
    {
        return new PrimaryAdjustmentAssignmentManager(
            store,
            PrimaryAdjustmentAssignmentRules.DefaultPrimaryAdjustmentAssignmentValidator(),
            clock,
            new List<string>(),
            PrimaryAdjustmentAssignmentEngine.DefaultPrimaryAdjustmentAssignmentEngine(flags));
    }
}
