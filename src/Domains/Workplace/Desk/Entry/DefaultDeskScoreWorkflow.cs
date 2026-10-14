namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default desk scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultDeskScoreWorkflow(IDefaultDeskScoreStore Store, DefaultDeskScoreValidator Validator, IClock Clock, List<string> Journal)
{
    public static DefaultDeskScoreWorkflow NewDefaultDeskScoreWorkflow(IDefaultDeskScoreStore store, IClock clock)
    {
        return new DefaultDeskScoreWorkflow(
            store,
            DefaultDeskScoreRules.DefaultDefaultDeskScoreValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default desk score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultDeskScoreStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default desk score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultDeskScoreStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
