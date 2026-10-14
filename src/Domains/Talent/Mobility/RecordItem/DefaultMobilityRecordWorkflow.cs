namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default mobility records through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultMobilityRecordDesk(IDefaultMobilityRecordStore Store, DefaultMobilityRecordValidator Validator, IClock Clock, List<string> Journal, DefaultMobilityRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default mobility record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultMobilityRecordStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default mobility record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultMobilityRecordStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default mobility record {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of default mobility record workflow.</summary>
public static class DefaultMobilityRecordWorkflow
{
    public static DefaultMobilityRecordDesk NewDefaultMobilityRecordDesk(IDefaultMobilityRecordStore store, IClock clock)
    {
        return new DefaultMobilityRecordDesk(
            store,
            DefaultMobilityRecordRules.DefaultDefaultMobilityRecordValidator(),
            clock,
            new List<string>(),
            DefaultMobilityRecordEngine.DefaultDefaultMobilityRecordEngine());
    }
}
