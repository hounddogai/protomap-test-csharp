namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy offboarding requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyOffboardingRequestService(ILegacyOffboardingRequestStore Store, LegacyOffboardingRequestValidator Validator, IClock Clock, List<string> Journal, LegacyOffboardingRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy offboarding request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyOffboardingRequestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy offboarding request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyOffboardingRequestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy offboarding request {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of legacy offboarding request workflow.</summary>
public static class LegacyOffboardingRequestWorkflow
{
    public static LegacyOffboardingRequestService NewLegacyOffboardingRequestService(ILegacyOffboardingRequestStore store, IClock clock)
    {
        return new LegacyOffboardingRequestService(
            store,
            LegacyOffboardingRequestRules.DefaultLegacyOffboardingRequestValidator(),
            clock,
            new List<string>(),
            LegacyOffboardingRequestEngine.DefaultLegacyOffboardingRequestEngine());
    }
}
