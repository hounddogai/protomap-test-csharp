namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default rate drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultRateDraftCoordinator(IDefaultRateDraftStore Store, DefaultRateDraftValidator Validator, IClock Clock, List<string> Journal, DefaultRateDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default rate draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultRateDraftStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default rate draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultRateDraftStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default rate draft {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of default rate draft workflow.</summary>
public static class DefaultRateDraftWorkflow
{
    public static DefaultRateDraftCoordinator NewDefaultRateDraftCoordinator(IDefaultRateDraftStore store, IClock clock, Registry flags)
    {
        return new DefaultRateDraftCoordinator(
            store,
            DefaultRateDraftRules.DefaultDefaultRateDraftValidator(),
            clock,
            new List<string>(),
            DefaultRateDraftEngine.DefaultDefaultRateDraftEngine(flags));
    }
}
