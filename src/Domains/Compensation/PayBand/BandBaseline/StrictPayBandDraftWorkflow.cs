namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict pay band drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictPayBandDraftWorkflow(IStrictPayBandDraftStore Store, StrictPayBandDraftValidator Validator, IClock Clock, List<string> Journal, StrictPayBandDraftEngine Engine)
{
    public static StrictPayBandDraftWorkflow NewStrictPayBandDraftWorkflow(IStrictPayBandDraftStore store, IClock clock)
    {
        return new StrictPayBandDraftWorkflow(
            store,
            StrictPayBandDraftRules.DefaultStrictPayBandDraftValidator(),
            clock,
            new List<string>(),
            StrictPayBandDraftEngine.DefaultStrictPayBandDraftEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pay band draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictPayBandDraftStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pay band draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictPayBandDraftStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pay band draft {code} not found");
        }
        var result = Engine.Price(item);
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
