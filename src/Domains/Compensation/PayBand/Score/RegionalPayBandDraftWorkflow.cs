namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional pay band drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalPayBandDraftDesk(IRegionalPayBandDraftStore Store, RegionalPayBandDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pay band draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalPayBandDraftStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pay band draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalPayBandDraftStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of regional pay band draft workflow.</summary>
public static class RegionalPayBandDraftWorkflow
{
    public static RegionalPayBandDraftDesk NewRegionalPayBandDraftDesk(IRegionalPayBandDraftStore store, IClock clock)
    {
        return new RegionalPayBandDraftDesk(
            store,
            RegionalPayBandDraftRules.DefaultRegionalPayBandDraftValidator(),
            clock,
            new List<string>());
    }
}
