namespace Acme.Workforce.Contractors.Rate.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted rate notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedRateNoticeDesk(IWeightedRateNoticeStore Store, WeightedRateNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted rate notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedRateNoticeStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted rate notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedRateNoticeStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of weighted rate notice workflow.</summary>
public static class WeightedRateNoticeWorkflow
{
    public static WeightedRateNoticeDesk NewWeightedRateNoticeDesk(IWeightedRateNoticeStore store, IClock clock)
    {
        return new WeightedRateNoticeDesk(
            store,
            WeightedRateNoticeRules.DefaultWeightedRateNoticeValidator(),
            clock,
            new List<string>());
    }
}
