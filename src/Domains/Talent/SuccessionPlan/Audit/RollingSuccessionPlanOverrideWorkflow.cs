namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling succession plan overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingSuccessionPlanOverrideDesk(IRollingSuccessionPlanOverrideStore Store, RollingSuccessionPlanOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling succession plan override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingSuccessionPlanOverrideStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling succession plan override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingSuccessionPlanOverrideStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of rolling succession plan override workflow.</summary>
public static class RollingSuccessionPlanOverrideWorkflow
{
    public static RollingSuccessionPlanOverrideDesk NewRollingSuccessionPlanOverrideDesk(IRollingSuccessionPlanOverrideStore store, IClock clock)
    {
        return new RollingSuccessionPlanOverrideDesk(
            store,
            RollingSuccessionPlanOverrideRules.DefaultRollingSuccessionPlanOverrideValidator(),
            clock,
            new List<string>());
    }
}
