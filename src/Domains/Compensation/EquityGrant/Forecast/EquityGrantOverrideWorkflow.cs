namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves equity grant overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantOverrideCoordinator(IEquityGrantOverrideStore Store, EquityGrantOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantOverrideStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantOverrideStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of equity grant override workflow.</summary>
public static class EquityGrantOverrideWorkflow
{
    public static EquityGrantOverrideCoordinator NewEquityGrantOverrideCoordinator(IEquityGrantOverrideStore store, IClock clock)
    {
        return new EquityGrantOverrideCoordinator(
            store,
            EquityGrantOverrideRules.DefaultEquityGrantOverrideValidator(),
            clock,
            new List<string>());
    }
}
