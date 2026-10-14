namespace Acme.Workforce.Workplace.FloorPlan.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy floor plan rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyFloorPlanRuleWorkflow(ILegacyFloorPlanRuleStore Store, LegacyFloorPlanRuleValidator Validator, IClock Clock, List<string> Journal, LegacyFloorPlanRuleEngine Engine)
{
    public static LegacyFloorPlanRuleWorkflow NewLegacyFloorPlanRuleWorkflow(ILegacyFloorPlanRuleStore store, IClock clock)
    {
        return new LegacyFloorPlanRuleWorkflow(
            store,
            LegacyFloorPlanRuleRules.DefaultLegacyFloorPlanRuleValidator(),
            clock,
            new List<string>(),
            LegacyFloorPlanRuleEngine.DefaultLegacyFloorPlanRuleEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy floor plan rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyFloorPlanRuleStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy floor plan rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyFloorPlanRuleStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy floor plan rule {code} not found");
        }
        var result = Engine.Evaluate(item);
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
