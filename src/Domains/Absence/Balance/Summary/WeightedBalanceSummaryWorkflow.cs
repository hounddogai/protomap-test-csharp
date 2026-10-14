namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted balance summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedBalanceSummaryWorkflow(IWeightedBalanceSummaryStore Store, WeightedBalanceSummaryValidator Validator, IClock Clock, List<string> Journal, WeightedBalanceSummaryEngine Engine)
{
    public static WeightedBalanceSummaryWorkflow NewWeightedBalanceSummaryWorkflow(IWeightedBalanceSummaryStore store, IClock clock)
    {
        return new WeightedBalanceSummaryWorkflow(
            store,
            WeightedBalanceSummaryRules.DefaultWeightedBalanceSummaryValidator(),
            clock,
            new List<string>(),
            WeightedBalanceSummaryEngine.DefaultWeightedBalanceSummaryEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted balance summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedBalanceSummaryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted balance summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedBalanceSummaryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted balance summary {code} not found");
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
