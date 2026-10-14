namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanExceptionWorkflow(IActionPlanExceptionStore Store, ActionPlanExceptionValidator Validator, IClock Clock, List<string> Journal, ActionPlanExceptionEngine Engine)
{
    public static ActionPlanExceptionWorkflow NewActionPlanExceptionWorkflow(IActionPlanExceptionStore store, IClock clock)
    {
        return new ActionPlanExceptionWorkflow(
            store,
            ActionPlanExceptionRules.DefaultActionPlanExceptionValidator(),
            clock,
            new List<string>(),
            ActionPlanExceptionEngine.DefaultActionPlanExceptionEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanExceptionStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanExceptionStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan exception {code} not found");
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
