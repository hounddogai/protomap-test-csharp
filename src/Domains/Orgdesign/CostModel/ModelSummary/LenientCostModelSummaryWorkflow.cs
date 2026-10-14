namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient cost model summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientCostModelSummaryDesk(ILenientCostModelSummaryStore Store, LenientCostModelSummaryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient cost model summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientCostModelSummaryStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient cost model summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientCostModelSummaryStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of lenient cost model summary workflow.</summary>
public static class LenientCostModelSummaryWorkflow
{
    public static LenientCostModelSummaryDesk NewLenientCostModelSummaryDesk(ILenientCostModelSummaryStore store, IClock clock)
    {
        return new LenientCostModelSummaryDesk(
            store,
            LenientCostModelSummaryRules.DefaultLenientCostModelSummaryValidator(),
            clock,
            new List<string>());
    }
}
