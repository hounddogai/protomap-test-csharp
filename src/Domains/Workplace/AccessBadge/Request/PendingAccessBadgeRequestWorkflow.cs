namespace Acme.Workforce.Workplace.AccessBadge.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending access badge requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingAccessBadgeRequestWorkflow(IPendingAccessBadgeRequestStore Store, PendingAccessBadgeRequestValidator Validator, IClock Clock, List<string> Journal)
{
    public static PendingAccessBadgeRequestWorkflow NewPendingAccessBadgeRequestWorkflow(IPendingAccessBadgeRequestStore store, IClock clock)
    {
        return new PendingAccessBadgeRequestWorkflow(
            store,
            PendingAccessBadgeRequestRules.DefaultPendingAccessBadgeRequestValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending access badge request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingAccessBadgeRequestStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending access badge request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingAccessBadgeRequestStatus.Archived);
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
