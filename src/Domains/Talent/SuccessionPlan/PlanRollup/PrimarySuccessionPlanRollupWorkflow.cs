namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary succession plan rollups through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimarySuccessionPlanRollupDesk(IPrimarySuccessionPlanRollupStore Store, PrimarySuccessionPlanRollupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary succession plan rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimarySuccessionPlanRollupStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary succession plan rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimarySuccessionPlanRollupStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of primary succession plan rollup workflow.</summary>
public static class PrimarySuccessionPlanRollupWorkflow
{
    public static PrimarySuccessionPlanRollupDesk NewPrimarySuccessionPlanRollupDesk(IPrimarySuccessionPlanRollupStore store, IClock clock)
    {
        return new PrimarySuccessionPlanRollupDesk(
            store,
            PrimarySuccessionPlanRollupRules.DefaultPrimarySuccessionPlanRollupValidator(),
            clock,
            new List<string>());
    }
}
