namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal pay band audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalPayBandAuditWorkflow(ISeasonalPayBandAuditStore Store, SeasonalPayBandAuditValidator Validator, IClock Clock, List<string> Journal, SeasonalPayBandAuditEngine Engine)
{
    public static SeasonalPayBandAuditWorkflow NewSeasonalPayBandAuditWorkflow(ISeasonalPayBandAuditStore store, IClock clock, Registry flags)
    {
        return new SeasonalPayBandAuditWorkflow(
            store,
            SeasonalPayBandAuditRules.DefaultSeasonalPayBandAuditValidator(),
            clock,
            new List<string>(),
            SeasonalPayBandAuditEngine.DefaultSeasonalPayBandAuditEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pay band audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalPayBandAuditStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pay band audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalPayBandAuditStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal pay band audit {code} not found");
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
