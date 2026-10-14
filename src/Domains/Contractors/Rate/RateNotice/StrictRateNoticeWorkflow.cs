namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict rate notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictRateNoticeWorkflow(IStrictRateNoticeStore Store, StrictRateNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    public static StrictRateNoticeWorkflow NewStrictRateNoticeWorkflow(IStrictRateNoticeStore store, IClock clock)
    {
        return new StrictRateNoticeWorkflow(
            store,
            StrictRateNoticeRules.DefaultStrictRateNoticeValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict rate notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictRateNoticeStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict rate notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictRateNoticeStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
            if (item.EndsAt > cutoff)
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
