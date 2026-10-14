namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves deduction notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionNoticeCoordinator(IDeductionNoticeStore Store, DeductionNoticeValidator Validator, IClock Clock, List<string> Journal, DeductionNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionNoticeStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionNoticeStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction notice {code} not found");
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

/// <summary>Functions of deduction notice workflow.</summary>
public static class DeductionNoticeWorkflow
{
    public static DeductionNoticeCoordinator NewDeductionNoticeCoordinator(IDeductionNoticeStore store, IClock clock, Registry flags)
    {
        return new DeductionNoticeCoordinator(
            store,
            DeductionNoticeRules.DefaultDeductionNoticeValidator(),
            clock,
            new List<string>(),
            DeductionNoticeEngine.DefaultDeductionNoticeEngine(flags));
    }
}
