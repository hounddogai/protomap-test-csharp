namespace Acme.Workforce.Compliance.Audit.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal audit assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalAuditAssignmentDesk(ISeasonalAuditAssignmentStore Store, SeasonalAuditAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal audit assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAuditAssignmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal audit assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAuditAssignmentStatus.Closed);
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

/// <summary>Functions of seasonal audit assignment workflow.</summary>
public static class SeasonalAuditAssignmentWorkflow
{
    public static SeasonalAuditAssignmentDesk NewSeasonalAuditAssignmentDesk(ISeasonalAuditAssignmentStore store, IClock clock)
    {
        return new SeasonalAuditAssignmentDesk(
            store,
            SeasonalAuditAssignmentRules.DefaultSeasonalAuditAssignmentValidator(),
            clock,
            new List<string>());
    }
}
