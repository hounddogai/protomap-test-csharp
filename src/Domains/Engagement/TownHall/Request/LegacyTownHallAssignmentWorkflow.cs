namespace Acme.Workforce.Engagement.TownHall.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy town hall assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyTownHallAssignmentWorkflow(ILegacyTownHallAssignmentStore Store, LegacyTownHallAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static LegacyTownHallAssignmentWorkflow NewLegacyTownHallAssignmentWorkflow(ILegacyTownHallAssignmentStore store, IClock clock)
    {
        return new LegacyTownHallAssignmentWorkflow(
            store,
            LegacyTownHallAssignmentRules.DefaultLegacyTownHallAssignmentValidator(),
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
            throw new InvalidOperationException($"legacy town hall assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyTownHallAssignmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy town hall assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyTownHallAssignmentStatus.Stopped);
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
