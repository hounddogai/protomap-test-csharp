namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorMilestoneWorkflow(IVisitorMilestoneStore Store, VisitorMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    public static VisitorMilestoneWorkflow NewVisitorMilestoneWorkflow(IVisitorMilestoneStore store, IClock clock)
    {
        return new VisitorMilestoneWorkflow(
            store,
            VisitorMilestoneRules.DefaultVisitorMilestoneValidator(),
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
            throw new InvalidOperationException($"visitor milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorMilestoneStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorMilestoneStatus.Stopped);
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
