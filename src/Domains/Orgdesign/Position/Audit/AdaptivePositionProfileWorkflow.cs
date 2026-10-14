namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive position profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptivePositionProfileService(IAdaptivePositionProfileStore Store, AdaptivePositionProfileValidator Validator, IClock Clock, List<string> Journal, AdaptivePositionProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive position profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptivePositionProfileStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive position profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptivePositionProfileStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive position profile {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of adaptive position profile workflow.</summary>
public static class AdaptivePositionProfileWorkflow
{
    public static AdaptivePositionProfileService NewAdaptivePositionProfileService(IAdaptivePositionProfileStore store, IClock clock, Registry flags)
    {
        return new AdaptivePositionProfileService(
            store,
            AdaptivePositionProfileRules.DefaultAdaptivePositionProfileValidator(),
            clock,
            new List<string>(),
            AdaptivePositionProfileEngine.DefaultAdaptivePositionProfileEngine(flags));
    }
}
