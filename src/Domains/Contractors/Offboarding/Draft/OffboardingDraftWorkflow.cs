namespace Acme.Workforce.Contractors.Offboarding.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves offboarding drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingDraftDesk(IOffboardingDraftStore Store, OffboardingDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingDraftStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingDraftStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of offboarding draft workflow.</summary>
public static class OffboardingDraftWorkflow
{
    public static OffboardingDraftDesk NewOffboardingDraftDesk(IOffboardingDraftStore store, IClock clock)
    {
        return new OffboardingDraftDesk(
            store,
            OffboardingDraftRules.DefaultOffboardingDraftValidator(),
            clock,
            new List<string>());
    }
}
