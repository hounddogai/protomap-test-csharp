namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves cost model variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelVarianceWorkflow(ICostModelVarianceStore Store, CostModelVarianceValidator Validator, IClock Clock, List<string> Journal, CostModelVarianceEngine Engine)
{
    public static CostModelVarianceWorkflow NewCostModelVarianceWorkflow(ICostModelVarianceStore store, IClock clock)
    {
        return new CostModelVarianceWorkflow(
            store,
            CostModelVarianceRules.DefaultCostModelVarianceValidator(),
            clock,
            new List<string>(),
            CostModelVarianceEngine.DefaultCostModelVarianceEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelVarianceStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelVarianceStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model variance {code} not found");
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
