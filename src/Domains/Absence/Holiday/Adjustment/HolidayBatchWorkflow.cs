namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves holiday batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayBatchCoordinator(IHolidayBatchStore Store, HolidayBatchValidator Validator, IClock Clock, List<string> Journal, HolidayBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayBatchStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayBatchStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday batch {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of holiday batch workflow.</summary>
public static class HolidayBatchWorkflow
{
    public static HolidayBatchCoordinator NewHolidayBatchCoordinator(IHolidayBatchStore store, IClock clock)
    {
        return new HolidayBatchCoordinator(
            store,
            HolidayBatchRules.DefaultHolidayBatchValidator(),
            clock,
            new List<string>(),
            HolidayBatchEngine.DefaultHolidayBatchEngine());
    }
}
