namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling recognition slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingRecognitionSlotCoordinator(IRollingRecognitionSlotStore Store, RollingRecognitionSlotValidator Validator, IClock Clock, List<string> Journal, RollingRecognitionSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling recognition slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingRecognitionSlotStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling recognition slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingRecognitionSlotStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling recognition slot {code} not found");
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

/// <summary>Functions of rolling recognition slot workflow.</summary>
public static class RollingRecognitionSlotWorkflow
{
    public static RollingRecognitionSlotCoordinator NewRollingRecognitionSlotCoordinator(IRollingRecognitionSlotStore store, IClock clock)
    {
        return new RollingRecognitionSlotCoordinator(
            store,
            RollingRecognitionSlotRules.DefaultRollingRecognitionSlotValidator(),
            clock,
            new List<string>(),
            RollingRecognitionSlotEngine.DefaultRollingRecognitionSlotEngine());
    }
}
