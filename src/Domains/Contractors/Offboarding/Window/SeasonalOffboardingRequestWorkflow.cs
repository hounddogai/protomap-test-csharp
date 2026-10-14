namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal offboarding requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalOffboardingRequestDesk(ISeasonalOffboardingRequestStore Store, SeasonalOffboardingRequestValidator Validator, IClock Clock, List<string> Journal, SeasonalOffboardingRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal offboarding request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalOffboardingRequestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal offboarding request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalOffboardingRequestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal offboarding request {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of seasonal offboarding request workflow.</summary>
public static class SeasonalOffboardingRequestWorkflow
{
    public static SeasonalOffboardingRequestDesk NewSeasonalOffboardingRequestDesk(ISeasonalOffboardingRequestStore store, IClock clock)
    {
        return new SeasonalOffboardingRequestDesk(
            store,
            SeasonalOffboardingRequestRules.DefaultSeasonalOffboardingRequestValidator(),
            clock,
            new List<string>(),
            SeasonalOffboardingRequestEngine.DefaultSeasonalOffboardingRequestEngine());
    }
}
