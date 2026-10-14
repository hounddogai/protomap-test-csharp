namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy recognition cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyRecognitionCycleDesk(ILegacyRecognitionCycleStore Store, LegacyRecognitionCycleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy recognition cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyRecognitionCycleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy recognition cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyRecognitionCycleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of legacy recognition cycle workflow.</summary>
public static class LegacyRecognitionCycleWorkflow
{
    public static LegacyRecognitionCycleDesk NewLegacyRecognitionCycleDesk(ILegacyRecognitionCycleStore store, IClock clock)
    {
        return new LegacyRecognitionCycleDesk(
            store,
            LegacyRecognitionCycleRules.DefaultLegacyRecognitionCycleValidator(),
            clock,
            new List<string>());
    }
}
