namespace Acme.Workforce.Absence.ReturnPlan.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered return plan items through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredReturnPlanItemWorkflow(ITieredReturnPlanItemStore Store, TieredReturnPlanItemValidator Validator, IClock Clock, List<string> Journal, TieredReturnPlanItemEngine Engine)
{
    public static TieredReturnPlanItemWorkflow NewTieredReturnPlanItemWorkflow(ITieredReturnPlanItemStore store, IClock clock, Registry flags)
    {
        return new TieredReturnPlanItemWorkflow(
            store,
            TieredReturnPlanItemRules.DefaultTieredReturnPlanItemValidator(),
            clock,
            new List<string>(),
            TieredReturnPlanItemEngine.DefaultTieredReturnPlanItemEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan item {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredReturnPlanItemStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan item {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredReturnPlanItemStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan item {code} not found");
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
