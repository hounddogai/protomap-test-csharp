namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves mobility assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilityAssignmentDesk(IMobilityAssignmentStore Store, MobilityAssignmentValidator Validator, IClock Clock, List<string> Journal, MobilityAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilityAssignmentStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilityAssignmentStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility assignment {code} not found");
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

/// <summary>Functions of mobility assignment workflow.</summary>
public static class MobilityAssignmentWorkflow
{
    public static MobilityAssignmentDesk NewMobilityAssignmentDesk(IMobilityAssignmentStore store, IClock clock)
    {
        return new MobilityAssignmentDesk(
            store,
            MobilityAssignmentRules.DefaultMobilityAssignmentValidator(),
            clock,
            new List<string>(),
            MobilityAssignmentEngine.DefaultMobilityAssignmentEngine());
    }
}
