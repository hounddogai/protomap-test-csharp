namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAdjustmentWorkflow(IPolicyAdjustmentStore Store, PolicyAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static PolicyAdjustmentWorkflow NewPolicyAdjustmentWorkflow(IPolicyAdjustmentStore store, IClock clock)
    {
        return new PolicyAdjustmentWorkflow(
            store,
            PolicyAdjustmentRules.DefaultPolicyAdjustmentValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAdjustmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAdjustmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
