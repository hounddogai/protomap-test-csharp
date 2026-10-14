namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted return plan overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedReturnPlanOverrideCoordinator(IWeightedReturnPlanOverrideStore Store, WeightedReturnPlanOverrideValidator Validator, IClock Clock, List<string> Journal, WeightedReturnPlanOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted return plan override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedReturnPlanOverrideStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted return plan override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedReturnPlanOverrideStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted return plan override {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of weighted return plan override workflow.</summary>
public static class WeightedReturnPlanOverrideWorkflow
{
    public static WeightedReturnPlanOverrideCoordinator NewWeightedReturnPlanOverrideCoordinator(IWeightedReturnPlanOverrideStore store, IClock clock)
    {
        return new WeightedReturnPlanOverrideCoordinator(
            store,
            WeightedReturnPlanOverrideRules.DefaultWeightedReturnPlanOverrideValidator(),
            clock,
            new List<string>(),
            WeightedReturnPlanOverrideEngine.DefaultWeightedReturnPlanOverrideEngine());
    }
}
