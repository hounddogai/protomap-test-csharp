namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy visitor checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyVisitorCheckpointWorkflow(ILegacyVisitorCheckpointStore Store, LegacyVisitorCheckpointValidator Validator, IClock Clock, List<string> Journal, LegacyVisitorCheckpointEngine Engine)
{
    public static LegacyVisitorCheckpointWorkflow NewLegacyVisitorCheckpointWorkflow(ILegacyVisitorCheckpointStore store, IClock clock)
    {
        return new LegacyVisitorCheckpointWorkflow(
            store,
            LegacyVisitorCheckpointRules.DefaultLegacyVisitorCheckpointValidator(),
            clock,
            new List<string>(),
            LegacyVisitorCheckpointEngine.DefaultLegacyVisitorCheckpointEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy visitor checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyVisitorCheckpointStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy visitor checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyVisitorCheckpointStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy visitor checkpoint {code} not found");
        }
        var result = Engine.Price(item);
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
