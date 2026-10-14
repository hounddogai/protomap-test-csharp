namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves audit thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditThresholdWorkflow(IAuditThresholdStore Store, AuditThresholdValidator Validator, IClock Clock, List<string> Journal, AuditThresholdEngine Engine)
{
    public static AuditThresholdWorkflow NewAuditThresholdWorkflow(IAuditThresholdStore store, IClock clock, Registry flags)
    {
        return new AuditThresholdWorkflow(
            store,
            AuditThresholdRules.DefaultAuditThresholdValidator(),
            clock,
            new List<string>(),
            AuditThresholdEngine.DefaultAuditThresholdEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditThresholdStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditThresholdStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit threshold {code} not found");
        }
        var result = Engine.Compute(item);
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
