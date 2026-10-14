namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy garnishment slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyGarnishmentSlotCoordinator(ILegacyGarnishmentSlotStore Store, LegacyGarnishmentSlotValidator Validator, IClock Clock, List<string> Journal, LegacyGarnishmentSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy garnishment slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyGarnishmentSlotStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy garnishment slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyGarnishmentSlotStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy garnishment slot {code} not found");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of legacy garnishment slot workflow.</summary>
public static class LegacyGarnishmentSlotWorkflow
{
    public static LegacyGarnishmentSlotCoordinator NewLegacyGarnishmentSlotCoordinator(ILegacyGarnishmentSlotStore store, IClock clock)
    {
        return new LegacyGarnishmentSlotCoordinator(
            store,
            LegacyGarnishmentSlotRules.DefaultLegacyGarnishmentSlotValidator(),
            clock,
            new List<string>(),
            LegacyGarnishmentSlotEngine.DefaultLegacyGarnishmentSlotEngine());
    }
}
