namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient position audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientPositionAuditService(ILenientPositionAuditStore Store, LenientPositionAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient position audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientPositionAuditStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient position audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientPositionAuditStatus.Archived);
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

/// <summary>Functions of lenient position audit workflow.</summary>
public static class LenientPositionAuditWorkflow
{
    public static LenientPositionAuditService NewLenientPositionAuditService(ILenientPositionAuditStore store, IClock clock)
    {
        return new LenientPositionAuditService(
            store,
            LenientPositionAuditRules.DefaultLenientPositionAuditValidator(),
            clock,
            new List<string>());
    }
}
