namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves engagement overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementOverrideDesk(IEngagementOverrideStore Store, EngagementOverrideValidator Validator, IClock Clock, List<string> Journal, EngagementOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementOverrideStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementOverrideStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement override {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of engagement override workflow.</summary>
public static class EngagementOverrideWorkflow
{
    public static EngagementOverrideDesk NewEngagementOverrideDesk(IEngagementOverrideStore store, IClock clock, Registry flags)
    {
        return new EngagementOverrideDesk(
            store,
            EngagementOverrideRules.DefaultEngagementOverrideValidator(),
            clock,
            new List<string>(),
            EngagementOverrideEngine.DefaultEngagementOverrideEngine(flags));
    }
}
