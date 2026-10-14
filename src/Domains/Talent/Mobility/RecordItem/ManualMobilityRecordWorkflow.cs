namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual mobility records through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualMobilityRecordCoordinator(IManualMobilityRecordStore Store, ManualMobilityRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual mobility record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualMobilityRecordStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual mobility record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualMobilityRecordStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of manual mobility record workflow.</summary>
public static class ManualMobilityRecordWorkflow
{
    public static ManualMobilityRecordCoordinator NewManualMobilityRecordCoordinator(IManualMobilityRecordStore store, IClock clock)
    {
        return new ManualMobilityRecordCoordinator(
            store,
            ManualMobilityRecordRules.DefaultManualMobilityRecordValidator(),
            clock,
            new List<string>());
    }
}
