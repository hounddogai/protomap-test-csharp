namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves bonus pool templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolTemplateService(IBonusPoolTemplateStore Store, BonusPoolTemplateValidator Validator, IClock Clock, List<string> Journal, BonusPoolTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolTemplateStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolTemplateStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool template {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of bonus pool template workflow.</summary>
public static class BonusPoolTemplateWorkflow
{
    public static BonusPoolTemplateService NewBonusPoolTemplateService(IBonusPoolTemplateStore store, IClock clock, Registry flags)
    {
        return new BonusPoolTemplateService(
            store,
            BonusPoolTemplateRules.DefaultBonusPoolTemplateValidator(),
            clock,
            new List<string>(),
            BonusPoolTemplateEngine.DefaultBonusPoolTemplateEngine(flags));
    }
}
