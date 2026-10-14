namespace Acme.Workforce.Contractors.Offboarding.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves offboarding claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingClaimDesk(IOffboardingClaimStore Store, OffboardingClaimValidator Validator, IClock Clock, List<string> Journal, OffboardingClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingClaimStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingClaimStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding claim {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of offboarding claim workflow.</summary>
public static class OffboardingClaimWorkflow
{
    public static OffboardingClaimDesk NewOffboardingClaimDesk(IOffboardingClaimStore store, IClock clock, Registry flags)
    {
        return new OffboardingClaimDesk(
            store,
            OffboardingClaimRules.DefaultOffboardingClaimValidator(),
            clock,
            new List<string>(),
            OffboardingClaimEngine.DefaultOffboardingClaimEngine(flags));
    }
}
