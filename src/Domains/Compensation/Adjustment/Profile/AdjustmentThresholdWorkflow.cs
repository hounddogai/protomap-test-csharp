namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentThresholdDesk(IAdjustmentThresholdStore Store, AdjustmentThresholdValidator Validator, IClock Clock, List<string> Journal, AdjustmentThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentThresholdStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentThresholdStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment threshold {code} not found");
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

/// <summary>Functions of adjustment threshold workflow.</summary>
public static class AdjustmentThresholdWorkflow
{
    public static AdjustmentThresholdDesk NewAdjustmentThresholdDesk(IAdjustmentThresholdStore store, IClock clock)
    {
        return new AdjustmentThresholdDesk(
            store,
            AdjustmentThresholdRules.DefaultAdjustmentThresholdValidator(),
            clock,
            new List<string>(),
            AdjustmentThresholdEngine.DefaultAdjustmentThresholdEngine());
    }
}
