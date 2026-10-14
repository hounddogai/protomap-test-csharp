namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending exception manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingExceptionManifestManager(IPendingExceptionManifestStore Store, PendingExceptionManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending exception manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingExceptionManifestStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending exception manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingExceptionManifestStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of pending exception manifest workflow.</summary>
public static class PendingExceptionManifestWorkflow
{
    public static PendingExceptionManifestManager NewPendingExceptionManifestManager(IPendingExceptionManifestStore store, IClock clock)
    {
        return new PendingExceptionManifestManager(
            store,
            PendingExceptionManifestRules.DefaultPendingExceptionManifestValidator(),
            clock,
            new List<string>());
    }
}
