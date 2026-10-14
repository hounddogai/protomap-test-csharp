namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves engagement plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementPlanWorkflow(IEngagementPlanStore Store, EngagementPlanValidator Validator, IClock Clock, List<string> Journal, EngagementPlanEngine Engine)
{
    public static EngagementPlanWorkflow NewEngagementPlanWorkflow(IEngagementPlanStore store, IClock clock, Registry flags)
    {
        return new EngagementPlanWorkflow(
            store,
            EngagementPlanRules.DefaultEngagementPlanValidator(),
            clock,
            new List<string>(),
            EngagementPlanEngine.DefaultEngagementPlanEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementPlanStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementPlanStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement plan {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
