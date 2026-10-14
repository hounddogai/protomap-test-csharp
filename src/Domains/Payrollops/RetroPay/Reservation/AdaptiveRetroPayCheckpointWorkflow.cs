namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive retro pay checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveRetroPayCheckpointWorkflow(IAdaptiveRetroPayCheckpointStore Store, AdaptiveRetroPayCheckpointValidator Validator, IClock Clock, List<string> Journal, AdaptiveRetroPayCheckpointEngine Engine)
{
    public static AdaptiveRetroPayCheckpointWorkflow NewAdaptiveRetroPayCheckpointWorkflow(IAdaptiveRetroPayCheckpointStore store, IClock clock, Registry flags)
    {
        return new AdaptiveRetroPayCheckpointWorkflow(
            store,
            AdaptiveRetroPayCheckpointRules.DefaultAdaptiveRetroPayCheckpointValidator(),
            clock,
            new List<string>(),
            AdaptiveRetroPayCheckpointEngine.DefaultAdaptiveRetroPayCheckpointEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive retro pay checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveRetroPayCheckpointStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive retro pay checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveRetroPayCheckpointStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive retro pay checkpoint {code} not found");
        }
        var result = Engine.Compute(item);
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
