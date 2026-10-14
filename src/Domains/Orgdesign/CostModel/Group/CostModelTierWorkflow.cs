namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves cost model tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelTierWorkflow(ICostModelTierStore Store, CostModelTierValidator Validator, IClock Clock, List<string> Journal, CostModelTierEngine Engine)
{
    public static CostModelTierWorkflow NewCostModelTierWorkflow(ICostModelTierStore store, IClock clock)
    {
        return new CostModelTierWorkflow(
            store,
            CostModelTierRules.DefaultCostModelTierValidator(),
            clock,
            new List<string>(),
            CostModelTierEngine.DefaultCostModelTierEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelTierStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelTierStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model tier {code} not found");
        }
        var result = Engine.Evaluate(item);
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
