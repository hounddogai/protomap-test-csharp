namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves offboarding plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingPlanService(IOffboardingPlanStore Store, OffboardingPlanValidator Validator, IClock Clock, List<string> Journal, OffboardingPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingPlanStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingPlanStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding plan {code} not found");
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

/// <summary>Functions of offboarding plan workflow.</summary>
public static class OffboardingPlanWorkflow
{
    public static OffboardingPlanService NewOffboardingPlanService(IOffboardingPlanStore store, IClock clock)
    {
        return new OffboardingPlanService(
            store,
            OffboardingPlanRules.DefaultOffboardingPlanValidator(),
            clock,
            new List<string>(),
            OffboardingPlanEngine.DefaultOffboardingPlanEngine());
    }
}
