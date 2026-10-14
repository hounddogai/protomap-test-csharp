namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional visitor baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalVisitorBaselineManager(IRegionalVisitorBaselineStore Store, RegionalVisitorBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional visitor baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalVisitorBaselineStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional visitor baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalVisitorBaselineStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of regional visitor baseline workflow.</summary>
public static class RegionalVisitorBaselineWorkflow
{
    public static RegionalVisitorBaselineManager NewRegionalVisitorBaselineManager(IRegionalVisitorBaselineStore store, IClock clock)
    {
        return new RegionalVisitorBaselineManager(
            store,
            RegionalVisitorBaselineRules.DefaultRegionalVisitorBaselineValidator(),
            clock,
            new List<string>());
    }
}
