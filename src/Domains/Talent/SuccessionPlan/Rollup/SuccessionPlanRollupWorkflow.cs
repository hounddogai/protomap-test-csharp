namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves succession plan rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record SuccessionPlanRollupManager(ISuccessionPlanRollupStore Store, SuccessionPlanRollupValidator Validator, IClock Clock, List<string> Journal, SuccessionPlanRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanRollupStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanRollupStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan rollup {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of succession plan rollup workflow.</summary>
public static class SuccessionPlanRollupWorkflow
{
    public static SuccessionPlanRollupManager NewSuccessionPlanRollupManager(ISuccessionPlanRollupStore store, IClock clock)
    {
        return new SuccessionPlanRollupManager(
            store,
            SuccessionPlanRollupRules.DefaultSuccessionPlanRollupValidator(),
            clock,
            new List<string>(),
            SuccessionPlanRollupEngine.DefaultSuccessionPlanRollupEngine());
    }
}
