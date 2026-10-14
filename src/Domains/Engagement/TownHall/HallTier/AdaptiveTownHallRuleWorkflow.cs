namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive town hall rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveTownHallRuleWorkflow(IAdaptiveTownHallRuleStore Store, AdaptiveTownHallRuleValidator Validator, IClock Clock, List<string> Journal, AdaptiveTownHallRuleEngine Engine)
{
    public static AdaptiveTownHallRuleWorkflow NewAdaptiveTownHallRuleWorkflow(IAdaptiveTownHallRuleStore store, IClock clock)
    {
        return new AdaptiveTownHallRuleWorkflow(
            store,
            AdaptiveTownHallRuleRules.DefaultAdaptiveTownHallRuleValidator(),
            clock,
            new List<string>(),
            AdaptiveTownHallRuleEngine.DefaultAdaptiveTownHallRuleEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive town hall rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveTownHallRuleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive town hall rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveTownHallRuleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive town hall rule {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.CreatedAt > cutoff)
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
