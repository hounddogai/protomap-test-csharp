namespace Acme.Workforce.Payrollops.PayRun.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay run checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayRunCheckpointWorkflow(IPayRunCheckpointStore Store, PayRunCheckpointValidator Validator, IClock Clock, List<string> Journal)
{
    public static PayRunCheckpointWorkflow NewPayRunCheckpointWorkflow(IPayRunCheckpointStore store, IClock clock)
    {
        return new PayRunCheckpointWorkflow(
            store,
            PayRunCheckpointRules.DefaultPayRunCheckpointValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayRunCheckpointStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayRunCheckpointStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
