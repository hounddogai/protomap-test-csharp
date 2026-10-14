namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped license seat checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedLicenseSeatCheckpointWorkflow(ICappedLicenseSeatCheckpointStore Store, CappedLicenseSeatCheckpointValidator Validator, IClock Clock, List<string> Journal, CappedLicenseSeatCheckpointEngine Engine)
{
    public static CappedLicenseSeatCheckpointWorkflow NewCappedLicenseSeatCheckpointWorkflow(ICappedLicenseSeatCheckpointStore store, IClock clock, Registry flags)
    {
        return new CappedLicenseSeatCheckpointWorkflow(
            store,
            CappedLicenseSeatCheckpointRules.DefaultCappedLicenseSeatCheckpointValidator(),
            clock,
            new List<string>(),
            CappedLicenseSeatCheckpointEngine.DefaultCappedLicenseSeatCheckpointEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped license seat checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedLicenseSeatCheckpointStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped license seat checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedLicenseSeatCheckpointStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped license seat checkpoint {code} not found");
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
