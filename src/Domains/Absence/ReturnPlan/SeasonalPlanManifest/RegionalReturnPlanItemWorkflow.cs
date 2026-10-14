namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional return plan items through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalReturnPlanItemWorkflow(IRegionalReturnPlanItemStore Store, RegionalReturnPlanItemValidator Validator, IClock Clock, List<string> Journal, RegionalReturnPlanItemEngine Engine)
{
    public static RegionalReturnPlanItemWorkflow NewRegionalReturnPlanItemWorkflow(IRegionalReturnPlanItemStore store, IClock clock)
    {
        return new RegionalReturnPlanItemWorkflow(
            store,
            RegionalReturnPlanItemRules.DefaultRegionalReturnPlanItemValidator(),
            clock,
            new List<string>(),
            RegionalReturnPlanItemEngine.DefaultRegionalReturnPlanItemEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional return plan item {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalReturnPlanItemStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional return plan item {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalReturnPlanItemStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional return plan item {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
