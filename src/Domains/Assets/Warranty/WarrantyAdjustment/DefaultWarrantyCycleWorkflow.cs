namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default warranty cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultWarrantyCycleManager(IDefaultWarrantyCycleStore Store, DefaultWarrantyCycleValidatorExtra Validator, IClock Clock, List<string> Journal, DefaultWarrantyCycleEngineExtra Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default warranty cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultWarrantyCycleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default warranty cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultWarrantyCycleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default warranty cycle {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of default warranty cycle workflow.</summary>
public static class DefaultWarrantyCycleWorkflow
{
    public static DefaultWarrantyCycleManager NewDefaultWarrantyCycleManager(IDefaultWarrantyCycleStore store, IClock clock, Registry flags)
    {
        return new DefaultWarrantyCycleManager(
            store,
            DefaultWarrantyCycleRules.DefaultDefaultWarrantyCycleValidator(),
            clock,
            new List<string>(),
            DefaultWarrantyCycleEngine.DefaultDefaultWarrantyCycleEngine(flags));
    }
}
