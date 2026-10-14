namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy floor plan notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyFloorPlanNoticeManager(ILegacyFloorPlanNoticeStore Store, LegacyFloorPlanNoticeValidator Validator, IClock Clock, List<string> Journal, LegacyFloorPlanNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy floor plan notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyFloorPlanNoticeStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy floor plan notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyFloorPlanNoticeStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy floor plan notice {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of legacy floor plan notice workflow.</summary>
public static class LegacyFloorPlanNoticeWorkflow
{
    public static LegacyFloorPlanNoticeManager NewLegacyFloorPlanNoticeManager(ILegacyFloorPlanNoticeStore store, IClock clock, Registry flags)
    {
        return new LegacyFloorPlanNoticeManager(
            store,
            LegacyFloorPlanNoticeRules.DefaultLegacyFloorPlanNoticeValidator(),
            clock,
            new List<string>(),
            LegacyFloorPlanNoticeEngine.DefaultLegacyFloorPlanNoticeEngine(flags));
    }
}
