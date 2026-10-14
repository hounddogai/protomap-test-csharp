namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient action plan exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientActionPlanExceptionService(ILenientActionPlanExceptionStore Store, LenientActionPlanExceptionValidator Validator, IClock Clock, List<string> Journal, LenientActionPlanExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient action plan exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientActionPlanExceptionStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient action plan exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientActionPlanExceptionStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient action plan exception {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of lenient action plan exception workflow.</summary>
public static class LenientActionPlanExceptionWorkflow
{
    public static LenientActionPlanExceptionService NewLenientActionPlanExceptionService(ILenientActionPlanExceptionStore store, IClock clock, Registry flags)
    {
        return new LenientActionPlanExceptionService(
            store,
            LenientActionPlanExceptionRules.DefaultLenientActionPlanExceptionValidator(),
            clock,
            new List<string>(),
            LenientActionPlanExceptionEngine.DefaultLenientActionPlanExceptionEngine(flags));
    }
}
