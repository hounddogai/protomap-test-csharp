namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global cost model groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalCostModelGroupDesk(IGlobalCostModelGroupStore Store, GlobalCostModelGroupValidator Validator, IClock Clock, List<string> Journal, GlobalCostModelGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global cost model group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalCostModelGroupStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global cost model group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalCostModelGroupStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global cost model group {code} not found");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of global cost model group workflow.</summary>
public static class GlobalCostModelGroupWorkflow
{
    public static GlobalCostModelGroupDesk NewGlobalCostModelGroupDesk(IGlobalCostModelGroupStore store, IClock clock, Registry flags)
    {
        return new GlobalCostModelGroupDesk(
            store,
            GlobalCostModelGroupRules.DefaultGlobalCostModelGroupValidator(),
            clock,
            new List<string>(),
            GlobalCostModelGroupEngine.DefaultGlobalCostModelGroupEngine(flags));
    }
}
