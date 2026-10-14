namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict return plan bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictReturnPlanBundleManager(IStrictReturnPlanBundleStore Store, StrictReturnPlanBundleValidator Validator, IClock Clock, List<string> Journal, StrictReturnPlanBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict return plan bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictReturnPlanBundleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict return plan bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictReturnPlanBundleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict return plan bundle {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of strict return plan bundle workflow.</summary>
public static class StrictReturnPlanBundleWorkflow
{
    public static StrictReturnPlanBundleManager NewStrictReturnPlanBundleManager(IStrictReturnPlanBundleStore store, IClock clock)
    {
        return new StrictReturnPlanBundleManager(
            store,
            StrictReturnPlanBundleRules.DefaultStrictReturnPlanBundleValidator(),
            clock,
            new List<string>(),
            StrictReturnPlanBundleEngine.DefaultStrictReturnPlanBundleEngine());
    }
}
