namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves visitor drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorDraftManager(IVisitorDraftStore Store, VisitorDraftValidator Validator, IClock Clock, List<string> Journal, VisitorDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorDraftStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorDraftStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor draft {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of visitor draft workflow.</summary>
public static class VisitorDraftWorkflow
{
    public static VisitorDraftManager NewVisitorDraftManager(IVisitorDraftStore store, IClock clock, Registry flags)
    {
        return new VisitorDraftManager(
            store,
            VisitorDraftRules.DefaultVisitorDraftValidator(),
            clock,
            new List<string>(),
            VisitorDraftEngine.DefaultVisitorDraftEngine(flags));
    }
}
