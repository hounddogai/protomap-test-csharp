namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy town hall checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyTownHallCheckpointWorkflow(ILegacyTownHallCheckpointStore Store, LegacyTownHallCheckpointValidator Validator, IClock Clock, List<string> Journal, LegacyTownHallCheckpointEngine Engine)
{
    public static LegacyTownHallCheckpointWorkflow NewLegacyTownHallCheckpointWorkflow(ILegacyTownHallCheckpointStore store, IClock clock, Registry flags)
    {
        return new LegacyTownHallCheckpointWorkflow(
            store,
            LegacyTownHallCheckpointRules.DefaultLegacyTownHallCheckpointValidator(),
            clock,
            new List<string>(),
            LegacyTownHallCheckpointEngine.DefaultLegacyTownHallCheckpointEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy town hall checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyTownHallCheckpointStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy town hall checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyTownHallCheckpointStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy town hall checkpoint {code} not found");
        }
        var result = Engine.Compute(item);
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
