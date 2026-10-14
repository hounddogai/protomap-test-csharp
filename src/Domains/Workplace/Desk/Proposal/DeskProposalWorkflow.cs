namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves desk proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskProposalCoordinator(IDeskProposalStore Store, DeskProposalValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskProposalStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskProposalStatus.Archived);
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

/// <summary>Functions of desk proposal workflow.</summary>
public static class DeskProposalWorkflow
{
    public static DeskProposalCoordinator NewDeskProposalCoordinator(IDeskProposalStore store, IClock clock)
    {
        return new DeskProposalCoordinator(
            store,
            DeskProposalRules.DefaultDeskProposalValidator(),
            clock,
            new List<string>());
    }
}
