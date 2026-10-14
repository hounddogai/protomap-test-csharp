namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves visitor profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorProfileCoordinator(IVisitorProfileStore Store, VisitorProfileValidator Validator, IClock Clock, List<string> Journal, VisitorProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorProfileStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorProfileStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor profile {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of visitor profile workflow.</summary>
public static class VisitorProfileWorkflow
{
    public static VisitorProfileCoordinator NewVisitorProfileCoordinator(IVisitorProfileStore store, IClock clock, Registry flags)
    {
        return new VisitorProfileCoordinator(
            store,
            VisitorProfileRules.DefaultVisitorProfileValidator(),
            clock,
            new List<string>(),
            VisitorProfileEngine.DefaultVisitorProfileEngine(flags));
    }
}
