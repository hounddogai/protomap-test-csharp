namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary policy ack drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryPolicyAckDraftDesk(IPrimaryPolicyAckDraftStore Store, PrimaryPolicyAckDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary policy ack draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryPolicyAckDraftStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary policy ack draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryPolicyAckDraftStatus.Archived);
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

/// <summary>Functions of primary policy ack draft workflow.</summary>
public static class PrimaryPolicyAckDraftWorkflow
{
    public static PrimaryPolicyAckDraftDesk NewPrimaryPolicyAckDraftDesk(IPrimaryPolicyAckDraftStore store, IClock clock)
    {
        return new PrimaryPolicyAckDraftDesk(
            store,
            PrimaryPolicyAckDraftRules.DefaultPrimaryPolicyAckDraftValidator(),
            clock,
            new List<string>());
    }
}
