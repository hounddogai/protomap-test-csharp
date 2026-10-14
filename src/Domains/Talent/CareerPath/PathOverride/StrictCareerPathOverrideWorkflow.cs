namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict career path overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictCareerPathOverrideCoordinator(IStrictCareerPathOverrideStore Store, StrictCareerPathOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict career path override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictCareerPathOverrideStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict career path override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictCareerPathOverrideStatus.Withdrawn);
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

/// <summary>Functions of strict career path override workflow.</summary>
public static class StrictCareerPathOverrideWorkflow
{
    public static StrictCareerPathOverrideCoordinator NewStrictCareerPathOverrideCoordinator(IStrictCareerPathOverrideStore store, IClock clock)
    {
        return new StrictCareerPathOverrideCoordinator(
            store,
            StrictCareerPathOverrideRules.DefaultStrictCareerPathOverrideValidator(),
            clock,
            new List<string>());
    }
}
