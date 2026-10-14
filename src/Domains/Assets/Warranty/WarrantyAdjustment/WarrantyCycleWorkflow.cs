namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves warranty cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyCycleCoordinator(IWarrantyCycleStore Store, WarrantyCycleValidator Validator, IClock Clock, List<string> Journal, WarrantyCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyCycleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyCycleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty cycle {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of warranty cycle workflow.</summary>
public static class WarrantyCycleWorkflow
{
    public static WarrantyCycleCoordinator NewWarrantyCycleCoordinator(IWarrantyCycleStore store, IClock clock, Registry flags)
    {
        return new WarrantyCycleCoordinator(
            store,
            WarrantyCycleRules.DefaultWarrantyCycleValidator(),
            clock,
            new List<string>(),
            WarrantyCycleEngine.DefaultWarrantyCycleEngine(flags));
    }
}
