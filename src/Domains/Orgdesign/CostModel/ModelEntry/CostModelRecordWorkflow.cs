namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves cost model records through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelRecordCoordinator(ICostModelRecordStore Store, CostModelRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelRecordStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelRecordStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of cost model record workflow.</summary>
public static class CostModelRecordWorkflow
{
    public static CostModelRecordCoordinator NewCostModelRecordCoordinator(ICostModelRecordStore store, IClock clock)
    {
        return new CostModelRecordCoordinator(
            store,
            CostModelRecordRules.DefaultCostModelRecordValidator(),
            clock,
            new List<string>());
    }
}
