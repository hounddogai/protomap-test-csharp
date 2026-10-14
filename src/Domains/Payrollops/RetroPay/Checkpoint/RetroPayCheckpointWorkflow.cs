namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayCheckpointWorkflow(IRetroPayCheckpointStore Store, RetroPayCheckpointValidator Validator, IClock Clock, List<string> Journal, RetroPayCheckpointEngine Engine)
{
    public static RetroPayCheckpointWorkflow NewRetroPayCheckpointWorkflow(IRetroPayCheckpointStore store, IClock clock)
    {
        return new RetroPayCheckpointWorkflow(
            store,
            RetroPayCheckpointRules.DefaultRetroPayCheckpointValidator(),
            clock,
            new List<string>(),
            RetroPayCheckpointEngine.DefaultRetroPayCheckpointEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayCheckpointStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayCheckpointStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay checkpoint {code} not found");
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
