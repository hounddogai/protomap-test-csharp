namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending succession plan targets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingSuccessionPlanTargetWorkflow(IPendingSuccessionPlanTargetStore Store, PendingSuccessionPlanTargetValidator Validator, IClock Clock, List<string> Journal, PendingSuccessionPlanTargetEngine Engine)
{
    public static PendingSuccessionPlanTargetWorkflow NewPendingSuccessionPlanTargetWorkflow(IPendingSuccessionPlanTargetStore store, IClock clock)
    {
        return new PendingSuccessionPlanTargetWorkflow(
            store,
            PendingSuccessionPlanTargetRules.DefaultPendingSuccessionPlanTargetValidator(),
            clock,
            new List<string>(),
            PendingSuccessionPlanTargetEngine.DefaultPendingSuccessionPlanTargetEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending succession plan target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingSuccessionPlanTargetStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending succession plan target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingSuccessionPlanTargetStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending succession plan target {code} not found");
        }
        var result = Engine.Run(item);
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
