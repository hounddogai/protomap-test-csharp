namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorManifestDesk(IVisitorManifestStore Store, VisitorManifestValidator Validator, IClock Clock, List<string> Journal, VisitorManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorManifestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorManifestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor manifest {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of visitor manifest workflow.</summary>
public static class VisitorManifestWorkflow
{
    public static VisitorManifestDesk NewVisitorManifestDesk(IVisitorManifestStore store, IClock clock)
    {
        return new VisitorManifestDesk(
            store,
            VisitorManifestRules.DefaultVisitorManifestValidator(),
            clock,
            new List<string>(),
            VisitorManifestEngine.DefaultVisitorManifestEngine());
    }
}
