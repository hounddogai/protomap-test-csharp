namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped audit plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedAuditPlanService(ICappedAuditPlanStore Store, CappedAuditPlanValidator Validator, IClock Clock, List<string> Journal, CappedAuditPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped audit plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedAuditPlanStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped audit plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedAuditPlanStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped audit plan {code} not found");
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

/// <summary>Functions of capped audit plan workflow.</summary>
public static class CappedAuditPlanWorkflow
{
    public static CappedAuditPlanService NewCappedAuditPlanService(ICappedAuditPlanStore store, IClock clock, Registry flags)
    {
        return new CappedAuditPlanService(
            store,
            CappedAuditPlanRules.DefaultCappedAuditPlanValidator(),
            clock,
            new List<string>(),
            CappedAuditPlanEngine.DefaultCappedAuditPlanEngine(flags));
    }
}
