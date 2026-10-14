namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary position plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryPositionPlanCoordinator(IPrimaryPositionPlanStore Store, PrimaryPositionPlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary position plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryPositionPlanStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary position plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryPositionPlanStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of primary position plan workflow.</summary>
public static class PrimaryPositionPlanWorkflow
{
    public static PrimaryPositionPlanCoordinator NewPrimaryPositionPlanCoordinator(IPrimaryPositionPlanStore store, IClock clock)
    {
        return new PrimaryPositionPlanCoordinator(
            store,
            PrimaryPositionPlanRules.DefaultPrimaryPositionPlanValidator(),
            clock,
            new List<string>());
    }
}
