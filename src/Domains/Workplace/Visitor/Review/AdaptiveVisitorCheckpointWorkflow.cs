namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive visitor checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveVisitorCheckpointCoordinator(IAdaptiveVisitorCheckpointStore Store, AdaptiveVisitorCheckpointValidator Validator, IClock Clock, List<string> Journal, AdaptiveVisitorCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive visitor checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveVisitorCheckpointStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive visitor checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveVisitorCheckpointStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive visitor checkpoint {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of adaptive visitor checkpoint workflow.</summary>
public static class AdaptiveVisitorCheckpointWorkflow
{
    public static AdaptiveVisitorCheckpointCoordinator NewAdaptiveVisitorCheckpointCoordinator(IAdaptiveVisitorCheckpointStore store, IClock clock)
    {
        return new AdaptiveVisitorCheckpointCoordinator(
            store,
            AdaptiveVisitorCheckpointRules.DefaultAdaptiveVisitorCheckpointValidator(),
            clock,
            new List<string>(),
            AdaptiveVisitorCheckpointEngine.DefaultAdaptiveVisitorCheckpointEngine());
    }
}
