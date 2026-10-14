namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves cost model slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelSlotWorkflow(ICostModelSlotStore Store, CostModelSlotValidator Validator, IClock Clock, List<string> Journal, CostModelSlotEngine Engine)
{
    public static CostModelSlotWorkflow NewCostModelSlotWorkflow(ICostModelSlotStore store, IClock clock)
    {
        return new CostModelSlotWorkflow(
            store,
            CostModelSlotRules.DefaultCostModelSlotValidator(),
            clock,
            new List<string>(),
            CostModelSlotEngine.DefaultCostModelSlotEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelSlotStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelSlotStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model slot {code} not found");
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
            if (item.StartsAt > cutoff)
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
