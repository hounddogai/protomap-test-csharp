namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentNoticeCoordinator(IAdjustmentNoticeStore Store, AdjustmentNoticeValidator Validator, IClock Clock, List<string> Journal, AdjustmentNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentNoticeStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentNoticeStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment notice {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of adjustment notice workflow.</summary>
public static class AdjustmentNoticeWorkflow
{
    public static AdjustmentNoticeCoordinator NewAdjustmentNoticeCoordinator(IAdjustmentNoticeStore store, IClock clock)
    {
        return new AdjustmentNoticeCoordinator(
            store,
            AdjustmentNoticeRules.DefaultAdjustmentNoticeValidator(),
            clock,
            new List<string>(),
            AdjustmentNoticeEngine.DefaultAdjustmentNoticeEngine());
    }
}
