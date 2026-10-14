namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive equity grant targets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveEquityGrantTargetManager(IAdaptiveEquityGrantTargetStore Store, AdaptiveEquityGrantTargetValidator Validator, IClock Clock, List<string> Journal, AdaptiveEquityGrantTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive equity grant target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveEquityGrantTargetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive equity grant target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveEquityGrantTargetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive equity grant target {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of adaptive equity grant target workflow.</summary>
public static class AdaptiveEquityGrantTargetWorkflow
{
    public static AdaptiveEquityGrantTargetManager NewAdaptiveEquityGrantTargetManager(IAdaptiveEquityGrantTargetStore store, IClock clock)
    {
        return new AdaptiveEquityGrantTargetManager(
            store,
            AdaptiveEquityGrantTargetRules.DefaultAdaptiveEquityGrantTargetValidator(),
            clock,
            new List<string>(),
            AdaptiveEquityGrantTargetEngine.DefaultAdaptiveEquityGrantTargetEngine());
    }
}
