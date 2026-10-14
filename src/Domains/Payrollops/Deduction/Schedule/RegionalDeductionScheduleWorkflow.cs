namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional deduction schedules through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalDeductionScheduleManager(IRegionalDeductionScheduleStore Store, RegionalDeductionScheduleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional deduction schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalDeductionScheduleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional deduction schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalDeductionScheduleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of regional deduction schedule workflow.</summary>
public static class RegionalDeductionScheduleWorkflow
{
    public static RegionalDeductionScheduleManager NewRegionalDeductionScheduleManager(IRegionalDeductionScheduleStore store, IClock clock)
    {
        return new RegionalDeductionScheduleManager(
            store,
            RegionalDeductionScheduleRules.DefaultRegionalDeductionScheduleValidator(),
            clock,
            new List<string>());
    }
}
