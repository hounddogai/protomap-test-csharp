namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered span drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredSpanDraftWorkflow(ITieredSpanDraftStore Store, TieredSpanDraftValidator Validator, IClock Clock, List<string> Journal, TieredSpanDraftEngine Engine)
{
    public static TieredSpanDraftWorkflow NewTieredSpanDraftWorkflow(ITieredSpanDraftStore store, IClock clock, Registry flags)
    {
        return new TieredSpanDraftWorkflow(
            store,
            TieredSpanDraftRules.DefaultTieredSpanDraftValidator(),
            clock,
            new List<string>(),
            TieredSpanDraftEngine.DefaultTieredSpanDraftEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered span draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredSpanDraftStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered span draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredSpanDraftStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered span draft {code} not found");
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
