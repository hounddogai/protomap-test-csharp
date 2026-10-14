namespace Acme.Workforce.Assets.Assignment.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves assignment plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentPlanWorkflow(IAssignmentPlanStore Store, AssignmentPlanValidator Validator, IClock Clock, List<string> Journal, AssignmentPlanEngine Engine)
{
    public static AssignmentPlanWorkflow NewAssignmentPlanWorkflow(IAssignmentPlanStore store, IClock clock)
    {
        return new AssignmentPlanWorkflow(
            store,
            AssignmentPlanRules.DefaultAssignmentPlanValidator(),
            clock,
            new List<string>(),
            AssignmentPlanEngine.DefaultAssignmentPlanEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentPlanStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentPlanStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment plan {code} not found");
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
