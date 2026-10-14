namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves succession plan targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record SuccessionPlanTargetManager(ISuccessionPlanTargetStore Store, SuccessionPlanTargetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanTargetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanTargetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of succession plan target workflow.</summary>
public static class SuccessionPlanTargetWorkflow
{
    public static SuccessionPlanTargetManager NewSuccessionPlanTargetManager(ISuccessionPlanTargetStore store, IClock clock)
    {
        return new SuccessionPlanTargetManager(
            store,
            SuccessionPlanTargetRules.DefaultSuccessionPlanTargetValidator(),
            clock,
            new List<string>());
    }
}
