namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal adjustment summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalAdjustmentSummaryCoordinator(ISeasonalAdjustmentSummaryStore Store, SeasonalAdjustmentSummaryValidator Validator, IClock Clock, List<string> Journal, SeasonalAdjustmentSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal adjustment summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAdjustmentSummaryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal adjustment summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAdjustmentSummaryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal adjustment summary {code} not found");
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

/// <summary>Functions of seasonal adjustment summary workflow.</summary>
public static class SeasonalAdjustmentSummaryWorkflow
{
    public static SeasonalAdjustmentSummaryCoordinator NewSeasonalAdjustmentSummaryCoordinator(ISeasonalAdjustmentSummaryStore store, IClock clock)
    {
        return new SeasonalAdjustmentSummaryCoordinator(
            store,
            SeasonalAdjustmentSummaryRules.DefaultSeasonalAdjustmentSummaryValidator(),
            clock,
            new List<string>(),
            SeasonalAdjustmentSummaryEngine.DefaultSeasonalAdjustmentSummaryEngine());
    }
}
