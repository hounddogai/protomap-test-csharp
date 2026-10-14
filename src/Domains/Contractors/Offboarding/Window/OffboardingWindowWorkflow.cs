namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves offboarding windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingWindowManager(IOffboardingWindowStore Store, OffboardingWindowValidator Validator, IClock Clock, List<string> Journal, OffboardingWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingWindowStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingWindowStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding window {code} not found");
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

/// <summary>Functions of offboarding window workflow.</summary>
public static class OffboardingWindowWorkflow
{
    public static OffboardingWindowManager NewOffboardingWindowManager(IOffboardingWindowStore store, IClock clock)
    {
        return new OffboardingWindowManager(
            store,
            OffboardingWindowRules.DefaultOffboardingWindowValidator(),
            clock,
            new List<string>(),
            OffboardingWindowEngine.DefaultOffboardingWindowEngine());
    }
}
