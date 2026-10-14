namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorEntryCoordinator(IVisitorEntryStore Store, VisitorEntryValidator Validator, IClock Clock, List<string> Journal, VisitorEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorEntryStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorEntryStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor entry {code} not found");
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

/// <summary>Functions of visitor entry workflow.</summary>
public static class VisitorEntryWorkflow
{
    public static VisitorEntryCoordinator NewVisitorEntryCoordinator(IVisitorEntryStore store, IClock clock)
    {
        return new VisitorEntryCoordinator(
            store,
            VisitorEntryRules.DefaultVisitorEntryValidator(),
            clock,
            new List<string>(),
            VisitorEntryEngine.DefaultVisitorEntryEngine());
    }
}
