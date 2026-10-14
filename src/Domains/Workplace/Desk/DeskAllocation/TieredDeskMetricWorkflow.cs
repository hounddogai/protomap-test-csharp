namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered desk metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredDeskMetricWorkflow(ITieredDeskMetricStore Store, TieredDeskMetricValidator Validator, IClock Clock, List<string> Journal, TieredDeskMetricEngine Engine)
{
    public static TieredDeskMetricWorkflow NewTieredDeskMetricWorkflow(ITieredDeskMetricStore store, IClock clock, Registry flags)
    {
        return new TieredDeskMetricWorkflow(
            store,
            TieredDeskMetricRules.DefaultTieredDeskMetricValidator(),
            clock,
            new List<string>(),
            TieredDeskMetricEngine.DefaultTieredDeskMetricEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered desk metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredDeskMetricStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered desk metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredDeskMetricStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered desk metric {code} not found");
        }
        var result = Engine.Run(item);
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
