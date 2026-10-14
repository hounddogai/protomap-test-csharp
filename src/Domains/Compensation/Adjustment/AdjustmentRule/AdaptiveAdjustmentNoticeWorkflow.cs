namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive adjustment notices through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveAdjustmentNoticeManager(IAdaptiveAdjustmentNoticeStore Store, AdaptiveAdjustmentNoticeValidator Validator, IClock Clock, List<string> Journal, AdaptiveAdjustmentNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive adjustment notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveAdjustmentNoticeStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive adjustment notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveAdjustmentNoticeStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive adjustment notice {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of adaptive adjustment notice workflow.</summary>
public static class AdaptiveAdjustmentNoticeWorkflow
{
    public static AdaptiveAdjustmentNoticeManager NewAdaptiveAdjustmentNoticeManager(IAdaptiveAdjustmentNoticeStore store, IClock clock)
    {
        return new AdaptiveAdjustmentNoticeManager(
            store,
            AdaptiveAdjustmentNoticeRules.DefaultAdaptiveAdjustmentNoticeValidator(),
            clock,
            new List<string>(),
            AdaptiveAdjustmentNoticeEngine.DefaultAdaptiveAdjustmentNoticeEngine());
    }
}
