namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict span proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictSpanProposalCoordinator(IStrictSpanProposalStore Store, StrictSpanProposalValidator Validator, IClock Clock, List<string> Journal, StrictSpanProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict span proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictSpanProposalStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict span proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictSpanProposalStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict span proposal {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of strict span proposal workflow.</summary>
public static class StrictSpanProposalWorkflow
{
    public static StrictSpanProposalCoordinator NewStrictSpanProposalCoordinator(IStrictSpanProposalStore store, IClock clock, Registry flags)
    {
        return new StrictSpanProposalCoordinator(
            store,
            StrictSpanProposalRules.DefaultStrictSpanProposalValidator(),
            clock,
            new List<string>(),
            StrictSpanProposalEngine.DefaultStrictSpanProposalEngine(flags));
    }
}
