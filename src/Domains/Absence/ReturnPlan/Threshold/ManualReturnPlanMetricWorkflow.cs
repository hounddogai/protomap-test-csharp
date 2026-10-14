namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual return plan metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualReturnPlanMetricWorkflow(IManualReturnPlanMetricStore Store, ManualReturnPlanMetricValidator Validator, IClock Clock, List<string> Journal)
{
    public static ManualReturnPlanMetricWorkflow NewManualReturnPlanMetricWorkflow(IManualReturnPlanMetricStore store, IClock clock)
    {
        return new ManualReturnPlanMetricWorkflow(
            store,
            ManualReturnPlanMetricRules.DefaultManualReturnPlanMetricValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual return plan metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualReturnPlanMetricStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual return plan metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualReturnPlanMetricStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
            if (item.ExpiresAt > cutoff)
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
