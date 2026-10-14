namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional cost model summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalCostModelSummaryDesk(IRegionalCostModelSummaryStore Store, RegionalCostModelSummaryValidator Validator, IClock Clock, List<string> Journal, RegionalCostModelSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional cost model summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalCostModelSummaryStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional cost model summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalCostModelSummaryStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional cost model summary {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of regional cost model summary workflow.</summary>
public static class RegionalCostModelSummaryWorkflow
{
    public static RegionalCostModelSummaryDesk NewRegionalCostModelSummaryDesk(IRegionalCostModelSummaryStore store, IClock clock)
    {
        return new RegionalCostModelSummaryDesk(
            store,
            RegionalCostModelSummaryRules.DefaultRegionalCostModelSummaryValidator(),
            clock,
            new List<string>(),
            RegionalCostModelSummaryEngine.DefaultRegionalCostModelSummaryEngine());
    }
}
