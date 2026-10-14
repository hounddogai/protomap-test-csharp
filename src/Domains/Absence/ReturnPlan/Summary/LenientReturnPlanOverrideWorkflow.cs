namespace Acme.Workforce.Absence.ReturnPlan.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient return plan overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientReturnPlanOverrideDesk(ILenientReturnPlanOverrideStore Store, LenientReturnPlanOverrideValidator Validator, IClock Clock, List<string> Journal, LenientReturnPlanOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient return plan override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientReturnPlanOverrideStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient return plan override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientReturnPlanOverrideStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient return plan override {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of lenient return plan override workflow.</summary>
public static class LenientReturnPlanOverrideWorkflow
{
    public static LenientReturnPlanOverrideDesk NewLenientReturnPlanOverrideDesk(ILenientReturnPlanOverrideStore store, IClock clock)
    {
        return new LenientReturnPlanOverrideDesk(
            store,
            LenientReturnPlanOverrideRules.DefaultLenientReturnPlanOverrideValidator(),
            clock,
            new List<string>(),
            LenientReturnPlanOverrideEngine.DefaultLenientReturnPlanOverrideEngine());
    }
}
