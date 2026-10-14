namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional access badge scores through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalAccessBadgeScoreWorkflow(IRegionalAccessBadgeScoreStore Store, RegionalAccessBadgeScoreValidator Validator, IClock Clock, List<string> Journal, RegionalAccessBadgeScoreEngine Engine)
{
    public static RegionalAccessBadgeScoreWorkflow NewRegionalAccessBadgeScoreWorkflow(IRegionalAccessBadgeScoreStore store, IClock clock)
    {
        return new RegionalAccessBadgeScoreWorkflow(
            store,
            RegionalAccessBadgeScoreRules.DefaultRegionalAccessBadgeScoreValidator(),
            clock,
            new List<string>(),
            RegionalAccessBadgeScoreEngine.DefaultRegionalAccessBadgeScoreEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional access badge score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalAccessBadgeScoreStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional access badge score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalAccessBadgeScoreStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional access badge score {code} not found");
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
