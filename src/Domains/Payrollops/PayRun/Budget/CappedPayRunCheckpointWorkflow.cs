namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped pay run checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedPayRunCheckpointWorkflow(ICappedPayRunCheckpointStore Store, CappedPayRunCheckpointValidator Validator, IClock Clock, List<string> Journal, CappedPayRunCheckpointEngine Engine)
{
    public static CappedPayRunCheckpointWorkflow NewCappedPayRunCheckpointWorkflow(ICappedPayRunCheckpointStore store, IClock clock, Registry flags)
    {
        return new CappedPayRunCheckpointWorkflow(
            store,
            CappedPayRunCheckpointRules.DefaultCappedPayRunCheckpointValidator(),
            clock,
            new List<string>(),
            CappedPayRunCheckpointEngine.DefaultCappedPayRunCheckpointEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped pay run checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedPayRunCheckpointStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped pay run checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedPayRunCheckpointStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped pay run checkpoint {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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
