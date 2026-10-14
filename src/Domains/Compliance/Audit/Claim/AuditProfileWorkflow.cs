namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves audit profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditProfileService(IAuditProfileStore Store, AuditProfileValidator Validator, IClock Clock, List<string> Journal, AuditProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditProfileStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditProfileStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit profile {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of audit profile workflow.</summary>
public static class AuditProfileWorkflow
{
    public static AuditProfileService NewAuditProfileService(IAuditProfileStore store, IClock clock, Registry flags)
    {
        return new AuditProfileService(
            store,
            AuditProfileRules.DefaultAuditProfileValidator(),
            clock,
            new List<string>(),
            AuditProfileEngine.DefaultAuditProfileEngine(flags));
    }
}
