namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary offboarding reviews through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryOffboardingReviewService(IPrimaryOffboardingReviewStore Store, PrimaryOffboardingReviewValidator Validator, IClock Clock, List<string> Journal, PrimaryOffboardingReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary offboarding review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryOffboardingReviewStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary offboarding review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryOffboardingReviewStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary offboarding review {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of primary offboarding review workflow.</summary>
public static class PrimaryOffboardingReviewWorkflow
{
    public static PrimaryOffboardingReviewService NewPrimaryOffboardingReviewService(IPrimaryOffboardingReviewStore store, IClock clock)
    {
        return new PrimaryOffboardingReviewService(
            store,
            PrimaryOffboardingReviewRules.DefaultPrimaryOffboardingReviewValidator(),
            clock,
            new List<string>(),
            PrimaryOffboardingReviewEngine.DefaultPrimaryOffboardingReviewEngine());
    }
}
