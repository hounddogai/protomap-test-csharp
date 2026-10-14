namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped offboarding thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedOffboardingThresholdWorkflow(ICappedOffboardingThresholdStore Store, CappedOffboardingThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    public static CappedOffboardingThresholdWorkflow NewCappedOffboardingThresholdWorkflow(ICappedOffboardingThresholdStore store, IClock clock)
    {
        return new CappedOffboardingThresholdWorkflow(
            store,
            CappedOffboardingThresholdRules.DefaultCappedOffboardingThresholdValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped offboarding threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedOffboardingThresholdStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped offboarding threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedOffboardingThresholdStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
