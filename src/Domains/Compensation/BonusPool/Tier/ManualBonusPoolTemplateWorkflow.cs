namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual bonus pool templates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualBonusPoolTemplateService(IManualBonusPoolTemplateStore Store, ManualBonusPoolTemplateValidator Validator, IClock Clock, List<string> Journal, ManualBonusPoolTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual bonus pool template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualBonusPoolTemplateStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual bonus pool template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualBonusPoolTemplateStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual bonus pool template {code} not found");
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

/// <summary>Functions of manual bonus pool template workflow.</summary>
public static class ManualBonusPoolTemplateWorkflow
{
    public static ManualBonusPoolTemplateService NewManualBonusPoolTemplateService(IManualBonusPoolTemplateStore store, IClock clock)
    {
        return new ManualBonusPoolTemplateService(
            store,
            ManualBonusPoolTemplateRules.DefaultManualBonusPoolTemplateValidator(),
            clock,
            new List<string>(),
            ManualBonusPoolTemplateEngine.DefaultManualBonusPoolTemplateEngine());
    }
}
