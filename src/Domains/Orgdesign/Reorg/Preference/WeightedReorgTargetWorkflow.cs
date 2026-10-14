namespace Acme.Workforce.Orgdesign.Reorg.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted reorg targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedReorgTargetManager(IWeightedReorgTargetStore Store, WeightedReorgTargetValidator Validator, IClock Clock, List<string> Journal, WeightedReorgTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted reorg target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedReorgTargetStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted reorg target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedReorgTargetStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted reorg target {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of weighted reorg target workflow.</summary>
public static class WeightedReorgTargetWorkflow
{
    public static WeightedReorgTargetManager NewWeightedReorgTargetManager(IWeightedReorgTargetStore store, IClock clock)
    {
        return new WeightedReorgTargetManager(
            store,
            WeightedReorgTargetRules.DefaultWeightedReorgTargetValidator(),
            clock,
            new List<string>(),
            WeightedReorgTargetEngine.DefaultWeightedReorgTargetEngine());
    }
}
