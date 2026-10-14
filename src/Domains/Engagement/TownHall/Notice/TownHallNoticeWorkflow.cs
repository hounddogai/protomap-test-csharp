namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves town hall notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallNoticeWorkflow(ITownHallNoticeStore Store, TownHallNoticeValidator Validator, IClock Clock, List<string> Journal, TownHallNoticeEngine Engine)
{
    public static TownHallNoticeWorkflow NewTownHallNoticeWorkflow(ITownHallNoticeStore store, IClock clock, Registry flags)
    {
        return new TownHallNoticeWorkflow(
            store,
            TownHallNoticeRules.DefaultTownHallNoticeValidator(),
            clock,
            new List<string>(),
            TownHallNoticeEngine.DefaultTownHallNoticeEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallNoticeStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallNoticeStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall notice {code} not found");
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
