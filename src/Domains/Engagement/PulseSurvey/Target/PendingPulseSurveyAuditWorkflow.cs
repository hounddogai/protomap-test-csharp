namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending pulse survey audits through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingPulseSurveyAuditManager(IPendingPulseSurveyAuditStore Store, PendingPulseSurveyAuditValidator Validator, IClock Clock, List<string> Journal, PendingPulseSurveyAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pulse survey audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingPulseSurveyAuditStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pulse survey audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingPulseSurveyAuditStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pulse survey audit {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of pending pulse survey audit workflow.</summary>
public static class PendingPulseSurveyAuditWorkflow
{
    public static PendingPulseSurveyAuditManager NewPendingPulseSurveyAuditManager(IPendingPulseSurveyAuditStore store, IClock clock, Registry flags)
    {
        return new PendingPulseSurveyAuditManager(
            store,
            PendingPulseSurveyAuditRules.DefaultPendingPulseSurveyAuditValidator(),
            clock,
            new List<string>(),
            PendingPulseSurveyAuditEngine.DefaultPendingPulseSurveyAuditEngine(flags));
    }
}
