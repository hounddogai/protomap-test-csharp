namespace Acme.Workforce.Absence.Accrual.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary accrual notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryAccrualNoticeManager(IPrimaryAccrualNoticeStore Store, PrimaryAccrualNoticeValidator Validator, IClock Clock, List<string> Journal, PrimaryAccrualNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary accrual notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryAccrualNoticeStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary accrual notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryAccrualNoticeStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary accrual notice {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of primary accrual notice workflow.</summary>
public static class PrimaryAccrualNoticeWorkflow
{
    public static PrimaryAccrualNoticeManager NewPrimaryAccrualNoticeManager(IPrimaryAccrualNoticeStore store, IClock clock)
    {
        return new PrimaryAccrualNoticeManager(
            store,
            PrimaryAccrualNoticeRules.DefaultPrimaryAccrualNoticeValidator(),
            clock,
            new List<string>(),
            PrimaryAccrualNoticeEngine.DefaultPrimaryAccrualNoticeEngine());
    }
}
