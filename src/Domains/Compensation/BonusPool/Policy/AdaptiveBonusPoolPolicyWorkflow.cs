namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive bonus pool policies through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveBonusPoolPolicyCoordinator(IAdaptiveBonusPoolPolicyStore Store, AdaptiveBonusPoolPolicyValidator Validator, IClock Clock, List<string> Journal, AdaptiveBonusPoolPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveBonusPoolPolicyStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveBonusPoolPolicyStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool policy {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of adaptive bonus pool policy workflow.</summary>
public static class AdaptiveBonusPoolPolicyWorkflow
{
    public static AdaptiveBonusPoolPolicyCoordinator NewAdaptiveBonusPoolPolicyCoordinator(IAdaptiveBonusPoolPolicyStore store, IClock clock)
    {
        return new AdaptiveBonusPoolPolicyCoordinator(
            store,
            AdaptiveBonusPoolPolicyRules.DefaultAdaptiveBonusPoolPolicyValidator(),
            clock,
            new List<string>(),
            AdaptiveBonusPoolPolicyEngine.DefaultAdaptiveBonusPoolPolicyEngine());
    }
}
