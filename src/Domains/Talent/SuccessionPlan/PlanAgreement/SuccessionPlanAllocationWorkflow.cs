namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves succession plan allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SuccessionPlanAllocationService(ISuccessionPlanAllocationStore Store, SuccessionPlanAllocationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanAllocationStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanAllocationStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of succession plan allocation workflow.</summary>
public static class SuccessionPlanAllocationWorkflow
{
    public static SuccessionPlanAllocationService NewSuccessionPlanAllocationService(ISuccessionPlanAllocationStore store, IClock clock)
    {
        return new SuccessionPlanAllocationService(
            store,
            SuccessionPlanAllocationRules.DefaultSuccessionPlanAllocationValidator(),
            clock,
            new List<string>());
    }
}
