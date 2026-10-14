namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves deduction schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionScheduleDesk(IDeductionScheduleStore Store, DeductionScheduleValidator Validator, IClock Clock, List<string> Journal, DeductionScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionScheduleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionScheduleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction schedule {code} not found");
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

/// <summary>Functions of deduction schedule workflow.</summary>
public static class DeductionScheduleWorkflow
{
    public static DeductionScheduleDesk NewDeductionScheduleDesk(IDeductionScheduleStore store, IClock clock, Registry flags)
    {
        return new DeductionScheduleDesk(
            store,
            DeductionScheduleRules.DefaultDeductionScheduleValidator(),
            clock,
            new List<string>(),
            DeductionScheduleEngine.DefaultDeductionScheduleEngine(flags));
    }
}
