namespace Acme.Workforce.Engagement.ActionPlan.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual action plan entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualActionPlanEntryWorkflow(IManualActionPlanEntryStore Store, ManualActionPlanEntryValidator Validator, IClock Clock, List<string> Journal)
{
    public static ManualActionPlanEntryWorkflow NewManualActionPlanEntryWorkflow(IManualActionPlanEntryStore store, IClock clock)
    {
        return new ManualActionPlanEntryWorkflow(
            store,
            ManualActionPlanEntryRules.DefaultManualActionPlanEntryValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual action plan entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualActionPlanEntryStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual action plan entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualActionPlanEntryStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
            if (item.UpdatedAt > cutoff)
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
