namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted visitor baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedVisitorBaselineManager(IWeightedVisitorBaselineStore Store, WeightedVisitorBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted visitor baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedVisitorBaselineStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted visitor baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedVisitorBaselineStatus.Archived);
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

/// <summary>Functions of weighted visitor baseline workflow.</summary>
public static class WeightedVisitorBaselineWorkflow
{
    public static WeightedVisitorBaselineManager NewWeightedVisitorBaselineManager(IWeightedVisitorBaselineStore store, IClock clock)
    {
        return new WeightedVisitorBaselineManager(
            store,
            WeightedVisitorBaselineRules.DefaultWeightedVisitorBaselineValidator(),
            clock,
            new List<string>());
    }
}
