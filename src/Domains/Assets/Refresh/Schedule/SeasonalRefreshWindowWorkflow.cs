namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal refresh windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalRefreshWindowWorkflow(ISeasonalRefreshWindowStore Store, SeasonalRefreshWindowValidator Validator, IClock Clock, List<string> Journal, SeasonalRefreshWindowEngine Engine)
{
    public static SeasonalRefreshWindowWorkflow NewSeasonalRefreshWindowWorkflow(ISeasonalRefreshWindowStore store, IClock clock)
    {
        return new SeasonalRefreshWindowWorkflow(
            store,
            SeasonalRefreshWindowRules.DefaultSeasonalRefreshWindowValidator(),
            clock,
            new List<string>(),
            SeasonalRefreshWindowEngine.DefaultSeasonalRefreshWindowEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal refresh window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalRefreshWindowStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal refresh window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalRefreshWindowStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal refresh window {code} not found");
        }
        var result = Engine.Price(item);
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
