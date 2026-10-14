namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves offboarding reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingReviewService(IOffboardingReviewStore Store, OffboardingReviewValidator Validator, IClock Clock, List<string> Journal, OffboardingReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingReviewStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingReviewStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding review {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of offboarding review workflow.</summary>
public static class OffboardingReviewWorkflow
{
    public static OffboardingReviewService NewOffboardingReviewService(IOffboardingReviewStore store, IClock clock)
    {
        return new OffboardingReviewService(
            store,
            OffboardingReviewRules.DefaultOffboardingReviewValidator(),
            clock,
            new List<string>(),
            OffboardingReviewEngine.DefaultOffboardingReviewEngine());
    }
}
