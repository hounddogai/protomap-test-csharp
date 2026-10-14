namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional span drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalSpanDraftWorkflow(IRegionalSpanDraftStore Store, RegionalSpanDraftValidator Validator, IClock Clock, List<string> Journal, RegionalSpanDraftEngine Engine)
{
    public static RegionalSpanDraftWorkflow NewRegionalSpanDraftWorkflow(IRegionalSpanDraftStore store, IClock clock, Registry flags)
    {
        return new RegionalSpanDraftWorkflow(
            store,
            RegionalSpanDraftRules.DefaultRegionalSpanDraftValidator(),
            clock,
            new List<string>(),
            RegionalSpanDraftEngine.DefaultRegionalSpanDraftEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional span draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalSpanDraftStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional span draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalSpanDraftStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional span draft {code} not found");
        }
        var result = Engine.Run(item);
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
