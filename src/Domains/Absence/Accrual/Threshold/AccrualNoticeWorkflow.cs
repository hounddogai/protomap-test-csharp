namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualNoticeService(IAccrualNoticeStore Store, AccrualNoticeValidator Validator, IClock Clock, List<string> Journal, AccrualNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualNoticeStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualNoticeStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual notice {code} not found");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of accrual notice workflow.</summary>
public static class AccrualNoticeWorkflow
{
    public static AccrualNoticeService NewAccrualNoticeService(IAccrualNoticeStore store, IClock clock)
    {
        return new AccrualNoticeService(
            store,
            AccrualNoticeRules.DefaultAccrualNoticeValidator(),
            clock,
            new List<string>(),
            AccrualNoticeEngine.DefaultAccrualNoticeEngine());
    }
}
