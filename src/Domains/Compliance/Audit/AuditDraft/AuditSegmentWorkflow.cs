namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves audit segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditSegmentWorkflow(IAuditSegmentStore Store, AuditSegmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static AuditSegmentWorkflow NewAuditSegmentWorkflow(IAuditSegmentStore store, IClock clock)
    {
        return new AuditSegmentWorkflow(
            store,
            AuditSegmentRules.DefaultAuditSegmentValidator(),
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
            throw new InvalidOperationException($"audit segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditSegmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditSegmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
