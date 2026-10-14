namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual holiday variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualHolidayVarianceManager(IManualHolidayVarianceStore Store, ManualHolidayVarianceValidator Validator, IClock Clock, List<string> Journal, ManualHolidayVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual holiday variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualHolidayVarianceStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual holiday variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualHolidayVarianceStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual holiday variance {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of manual holiday variance workflow.</summary>
public static class ManualHolidayVarianceWorkflow
{
    public static ManualHolidayVarianceManager NewManualHolidayVarianceManager(IManualHolidayVarianceStore store, IClock clock)
    {
        return new ManualHolidayVarianceManager(
            store,
            ManualHolidayVarianceRules.DefaultManualHolidayVarianceValidator(),
            clock,
            new List<string>(),
            ManualHolidayVarianceEngine.DefaultManualHolidayVarianceEngine());
    }
}
