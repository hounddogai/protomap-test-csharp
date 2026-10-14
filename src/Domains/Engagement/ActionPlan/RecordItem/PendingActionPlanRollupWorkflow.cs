namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending action plan rollups through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingActionPlanRollupWorkflow(IPendingActionPlanRollupStore Store, PendingActionPlanRollupValidator Validator, IClock Clock, List<string> Journal, PendingActionPlanRollupEngine Engine)
{
    public static PendingActionPlanRollupWorkflow NewPendingActionPlanRollupWorkflow(IPendingActionPlanRollupStore store, IClock clock, Registry flags)
    {
        return new PendingActionPlanRollupWorkflow(
            store,
            PendingActionPlanRollupRules.DefaultPendingActionPlanRollupValidator(),
            clock,
            new List<string>(),
            PendingActionPlanRollupEngine.DefaultPendingActionPlanRollupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending action plan rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingActionPlanRollupStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending action plan rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingActionPlanRollupStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending action plan rollup {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
