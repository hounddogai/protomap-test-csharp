namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional pay run groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalPayRunGroupCoordinator(IRegionalPayRunGroupStore Store, RegionalPayRunGroupValidator Validator, IClock Clock, List<string> Journal, RegionalPayRunGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pay run group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalPayRunGroupStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pay run group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalPayRunGroupStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pay run group {code} not found");
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

/// <summary>Functions of regional pay run group workflow.</summary>
public static class RegionalPayRunGroupWorkflow
{
    public static RegionalPayRunGroupCoordinator NewRegionalPayRunGroupCoordinator(IRegionalPayRunGroupStore store, IClock clock, Registry flags)
    {
        return new RegionalPayRunGroupCoordinator(
            store,
            RegionalPayRunGroupRules.DefaultRegionalPayRunGroupValidator(),
            clock,
            new List<string>(),
            RegionalPayRunGroupEngine.DefaultRegionalPayRunGroupEngine(flags));
    }
}
