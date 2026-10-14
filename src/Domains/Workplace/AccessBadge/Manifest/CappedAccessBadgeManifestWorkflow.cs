namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped access badge manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedAccessBadgeManifestService(ICappedAccessBadgeManifestStore Store, CappedAccessBadgeManifestValidator Validator, IClock Clock, List<string> Journal, CappedAccessBadgeManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped access badge manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedAccessBadgeManifestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped access badge manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedAccessBadgeManifestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped access badge manifest {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of capped access badge manifest workflow.</summary>
public static class CappedAccessBadgeManifestWorkflow
{
    public static CappedAccessBadgeManifestService NewCappedAccessBadgeManifestService(ICappedAccessBadgeManifestStore store, IClock clock)
    {
        return new CappedAccessBadgeManifestService(
            store,
            CappedAccessBadgeManifestRules.DefaultCappedAccessBadgeManifestValidator(),
            clock,
            new List<string>(),
            CappedAccessBadgeManifestEngine.DefaultCappedAccessBadgeManifestEngine());
    }
}
