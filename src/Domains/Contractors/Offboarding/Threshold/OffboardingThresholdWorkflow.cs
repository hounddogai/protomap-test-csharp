namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves offboarding thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingThresholdCoordinator(IOffboardingThresholdStore Store, OffboardingThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingThresholdStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingThresholdStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of offboarding threshold workflow.</summary>
public static class OffboardingThresholdWorkflow
{
    public static OffboardingThresholdCoordinator NewOffboardingThresholdCoordinator(IOffboardingThresholdStore store, IClock clock)
    {
        return new OffboardingThresholdCoordinator(
            store,
            OffboardingThresholdRules.DefaultOffboardingThresholdValidator(),
            clock,
            new List<string>());
    }
}
