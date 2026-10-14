namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves succession plan summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record SuccessionPlanSummaryDesk(ISuccessionPlanSummaryStore Store, SuccessionPlanSummaryValidator Validator, IClock Clock, List<string> Journal, SuccessionPlanSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanSummaryStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanSummaryStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan summary {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of succession plan summary workflow.</summary>
public static class SuccessionPlanSummaryWorkflow
{
    public static SuccessionPlanSummaryDesk NewSuccessionPlanSummaryDesk(ISuccessionPlanSummaryStore store, IClock clock, Registry flags)
    {
        return new SuccessionPlanSummaryDesk(
            store,
            SuccessionPlanSummaryRules.DefaultSuccessionPlanSummaryValidator(),
            clock,
            new List<string>(),
            SuccessionPlanSummaryEngine.DefaultSuccessionPlanSummaryEngine(flags));
    }
}
