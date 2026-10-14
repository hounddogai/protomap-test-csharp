namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves headcount slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountSlotCoordinator(IHeadcountSlotStore Store, HeadcountSlotValidator Validator, IClock Clock, List<string> Journal, HeadcountSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountSlotStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountSlotStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount slot {code} not found");
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

/// <summary>Functions of headcount slot workflow.</summary>
public static class HeadcountSlotWorkflow
{
    public static HeadcountSlotCoordinator NewHeadcountSlotCoordinator(IHeadcountSlotStore store, IClock clock)
    {
        return new HeadcountSlotCoordinator(
            store,
            HeadcountSlotRules.DefaultHeadcountSlotValidator(),
            clock,
            new List<string>(),
            HeadcountSlotEngine.DefaultHeadcountSlotEngine());
    }
}
