namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient action plan records through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientActionPlanRecordManager(ILenientActionPlanRecordStore Store, LenientActionPlanRecordValidator Validator, IClock Clock, List<string> Journal, LenientActionPlanRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient action plan record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientActionPlanRecordStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient action plan record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientActionPlanRecordStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient action plan record {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of lenient action plan record workflow.</summary>
public static class LenientActionPlanRecordWorkflow
{
    public static LenientActionPlanRecordManager NewLenientActionPlanRecordManager(ILenientActionPlanRecordStore store, IClock clock)
    {
        return new LenientActionPlanRecordManager(
            store,
            LenientActionPlanRecordRules.DefaultLenientActionPlanRecordValidator(),
            clock,
            new List<string>(),
            LenientActionPlanRecordEngine.DefaultLenientActionPlanRecordEngine());
    }
}
