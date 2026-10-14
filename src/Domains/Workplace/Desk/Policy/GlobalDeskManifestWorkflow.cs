namespace Acme.Workforce.Workplace.Desk.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global desk manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalDeskManifestDesk(IGlobalDeskManifestStore Store, GlobalDeskManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global desk manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalDeskManifestStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global desk manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalDeskManifestStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of global desk manifest workflow.</summary>
public static class GlobalDeskManifestWorkflow
{
    public static GlobalDeskManifestDesk NewGlobalDeskManifestDesk(IGlobalDeskManifestStore store, IClock clock)
    {
        return new GlobalDeskManifestDesk(
            store,
            GlobalDeskManifestRules.DefaultGlobalDeskManifestValidator(),
            clock,
            new List<string>());
    }
}
