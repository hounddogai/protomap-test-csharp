namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy action plan metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyActionPlanMetricService(ILegacyActionPlanMetricStore Store, LegacyActionPlanMetricValidator Validator, IClock Clock, List<string> Journal, LegacyActionPlanMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy action plan metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyActionPlanMetricStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy action plan metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyActionPlanMetricStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy action plan metric {code} not found");
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

/// <summary>Functions of legacy action plan metric workflow.</summary>
public static class LegacyActionPlanMetricWorkflow
{
    public static LegacyActionPlanMetricService NewLegacyActionPlanMetricService(ILegacyActionPlanMetricStore store, IClock clock, Registry flags)
    {
        return new LegacyActionPlanMetricService(
            store,
            LegacyActionPlanMetricRules.DefaultLegacyActionPlanMetricValidator(),
            clock,
            new List<string>(),
            LegacyActionPlanMetricEngine.DefaultLegacyActionPlanMetricEngine(flags));
    }
}
