namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient engagement manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientEngagementManifestManager(ILenientEngagementManifestStore Store, LenientEngagementManifestValidator Validator, IClock Clock, List<string> Journal, LenientEngagementManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient engagement manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientEngagementManifestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient engagement manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientEngagementManifestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient engagement manifest {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of lenient engagement manifest workflow.</summary>
public static class LenientEngagementManifestWorkflow
{
    public static LenientEngagementManifestManager NewLenientEngagementManifestManager(ILenientEngagementManifestStore store, IClock clock)
    {
        return new LenientEngagementManifestManager(
            store,
            LenientEngagementManifestRules.DefaultLenientEngagementManifestValidator(),
            clock,
            new List<string>(),
            LenientEngagementManifestEngine.DefaultLenientEngagementManifestEngine());
    }
}
