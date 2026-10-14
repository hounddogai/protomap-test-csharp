namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered license seat rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredLicenseSeatRuleDesk(ITieredLicenseSeatRuleStore Store, TieredLicenseSeatRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered license seat rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredLicenseSeatRuleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered license seat rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredLicenseSeatRuleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of tiered license seat rule workflow.</summary>
public static class TieredLicenseSeatRuleWorkflow
{
    public static TieredLicenseSeatRuleDesk NewTieredLicenseSeatRuleDesk(ITieredLicenseSeatRuleStore store, IClock clock)
    {
        return new TieredLicenseSeatRuleDesk(
            store,
            TieredLicenseSeatRuleRules.DefaultTieredLicenseSeatRuleValidator(),
            clock,
            new List<string>());
    }
}
