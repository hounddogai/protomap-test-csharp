namespace Acme.Workforce.Payrollops.PayRun.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient pay run schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientPayRunScheduleService(ILenientPayRunScheduleStore Store, LenientPayRunScheduleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient pay run schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientPayRunScheduleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient pay run schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientPayRunScheduleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of lenient pay run schedule workflow.</summary>
public static class LenientPayRunScheduleWorkflow
{
    public static LenientPayRunScheduleService NewLenientPayRunScheduleService(ILenientPayRunScheduleStore store, IClock clock)
    {
        return new LenientPayRunScheduleService(
            store,
            LenientPayRunScheduleRules.DefaultLenientPayRunScheduleValidator(),
            clock,
            new List<string>());
    }
}
