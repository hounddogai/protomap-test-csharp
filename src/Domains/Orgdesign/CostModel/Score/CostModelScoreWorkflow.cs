namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves cost model scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelScoreService(ICostModelScoreStore Store, CostModelScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelScoreStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelScoreStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of cost model score workflow.</summary>
public static class CostModelScoreWorkflow
{
    public static CostModelScoreService NewCostModelScoreService(ICostModelScoreStore store, IClock clock)
    {
        return new CostModelScoreService(
            store,
            CostModelScoreRules.DefaultCostModelScoreValidator(),
            clock,
            new List<string>());
    }
}
