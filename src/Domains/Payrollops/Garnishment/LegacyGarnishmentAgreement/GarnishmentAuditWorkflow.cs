namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves garnishment audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentAuditService(IGarnishmentAuditStore Store, GarnishmentAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentAuditStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentAuditStatus.Archived);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of garnishment audit workflow.</summary>
public static class GarnishmentAuditWorkflow
{
    public static GarnishmentAuditService NewGarnishmentAuditService(IGarnishmentAuditStore store, IClock clock)
    {
        return new GarnishmentAuditService(
            store,
            GarnishmentAuditRules.DefaultGarnishmentAuditValidator(),
            clock,
            new List<string>());
    }
}
