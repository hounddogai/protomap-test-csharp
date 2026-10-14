namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending town hall summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingTownHallSummaryWorkflow(IPendingTownHallSummaryStore Store, PendingTownHallSummaryValidator Validator, IClock Clock, List<string> Journal, PendingTownHallSummaryEngine Engine)
{
    public static PendingTownHallSummaryWorkflow NewPendingTownHallSummaryWorkflow(IPendingTownHallSummaryStore store, IClock clock)
    {
        return new PendingTownHallSummaryWorkflow(
            store,
            PendingTownHallSummaryRules.DefaultPendingTownHallSummaryValidator(),
            clock,
            new List<string>(),
            PendingTownHallSummaryEngine.DefaultPendingTownHallSummaryEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending town hall summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingTownHallSummaryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending town hall summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingTownHallSummaryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending town hall summary {code} not found");
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
