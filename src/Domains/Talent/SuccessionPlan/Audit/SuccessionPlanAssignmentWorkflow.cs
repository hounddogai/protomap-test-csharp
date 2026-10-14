namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves succession plan assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SuccessionPlanAssignmentDesk(ISuccessionPlanAssignmentStore Store, SuccessionPlanAssignmentValidator Validator, IClock Clock, List<string> Journal, SuccessionPlanAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanAssignmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanAssignmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan assignment {code} not found");
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

/// <summary>Functions of succession plan assignment workflow.</summary>
public static class SuccessionPlanAssignmentWorkflow
{
    public static SuccessionPlanAssignmentDesk NewSuccessionPlanAssignmentDesk(ISuccessionPlanAssignmentStore store, IClock clock, Registry flags)
    {
        return new SuccessionPlanAssignmentDesk(
            store,
            SuccessionPlanAssignmentRules.DefaultSuccessionPlanAssignmentValidator(),
            clock,
            new List<string>(),
            SuccessionPlanAssignmentEngine.DefaultSuccessionPlanAssignmentEngine(flags));
    }
}
