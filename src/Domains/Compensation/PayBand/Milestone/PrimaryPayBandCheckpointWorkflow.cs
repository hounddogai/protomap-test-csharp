namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary pay band checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryPayBandCheckpointWorkflow(IPrimaryPayBandCheckpointStore Store, PrimaryPayBandCheckpointValidator Validator, IClock Clock, List<string> Journal, PrimaryPayBandCheckpointEngine Engine)
{
    public static PrimaryPayBandCheckpointWorkflow NewPrimaryPayBandCheckpointWorkflow(IPrimaryPayBandCheckpointStore store, IClock clock, Registry flags)
    {
        return new PrimaryPayBandCheckpointWorkflow(
            store,
            PrimaryPayBandCheckpointRules.DefaultPrimaryPayBandCheckpointValidator(),
            clock,
            new List<string>(),
            PrimaryPayBandCheckpointEngine.DefaultPrimaryPayBandCheckpointEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary pay band checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryPayBandCheckpointStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary pay band checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryPayBandCheckpointStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary pay band checkpoint {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
