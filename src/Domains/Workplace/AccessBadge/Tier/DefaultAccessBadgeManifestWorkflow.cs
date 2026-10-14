namespace Acme.Workforce.Workplace.AccessBadge.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default access badge manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultAccessBadgeManifestService(IDefaultAccessBadgeManifestStore Store, DefaultAccessBadgeManifestValidator Validator, IClock Clock, List<string> Journal, DefaultAccessBadgeManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default access badge manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultAccessBadgeManifestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default access badge manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultAccessBadgeManifestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default access badge manifest {code} not found");
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

/// <summary>Functions of default access badge manifest workflow.</summary>
public static class DefaultAccessBadgeManifestWorkflow
{
    public static DefaultAccessBadgeManifestService NewDefaultAccessBadgeManifestService(IDefaultAccessBadgeManifestStore store, IClock clock)
    {
        return new DefaultAccessBadgeManifestService(
            store,
            DefaultAccessBadgeManifestRules.DefaultDefaultAccessBadgeManifestValidator(),
            clock,
            new List<string>(),
            DefaultAccessBadgeManifestEngine.DefaultDefaultAccessBadgeManifestEngine());
    }
}
