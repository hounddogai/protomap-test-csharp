namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient bonus pool slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientBonusPoolSlotService(ILenientBonusPoolSlotStore Store, LenientBonusPoolSlotValidator Validator, IClock Clock, List<string> Journal, LenientBonusPoolSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient bonus pool slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientBonusPoolSlotStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient bonus pool slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientBonusPoolSlotStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient bonus pool slot {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of lenient bonus pool slot workflow.</summary>
public static class LenientBonusPoolSlotWorkflow
{
    public static LenientBonusPoolSlotService NewLenientBonusPoolSlotService(ILenientBonusPoolSlotStore store, IClock clock)
    {
        return new LenientBonusPoolSlotService(
            store,
            LenientBonusPoolSlotRules.DefaultLenientBonusPoolSlotValidator(),
            clock,
            new List<string>(),
            LenientBonusPoolSlotEngine.DefaultLenientBonusPoolSlotEngine());
    }
}
