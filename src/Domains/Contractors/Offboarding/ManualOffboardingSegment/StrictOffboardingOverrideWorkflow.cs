namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict offboarding overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictOffboardingOverrideManager(IStrictOffboardingOverrideStore Store, StrictOffboardingOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict offboarding override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictOffboardingOverrideStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict offboarding override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictOffboardingOverrideStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of strict offboarding override workflow.</summary>
public static class StrictOffboardingOverrideWorkflow
{
    public static StrictOffboardingOverrideManager NewStrictOffboardingOverrideManager(IStrictOffboardingOverrideStore store, IClock clock)
    {
        return new StrictOffboardingOverrideManager(
            store,
            StrictOffboardingOverrideRules.DefaultStrictOffboardingOverrideValidator(),
            clock,
            new List<string>());
    }
}
