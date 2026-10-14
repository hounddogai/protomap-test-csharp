namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default adjustment agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultAdjustmentAgreementService(IDefaultAdjustmentAgreementStore Store, DefaultAdjustmentAgreementValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default adjustment agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultAdjustmentAgreementStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default adjustment agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultAdjustmentAgreementStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of default adjustment agreement workflow.</summary>
public static class DefaultAdjustmentAgreementWorkflow
{
    public static DefaultAdjustmentAgreementService NewDefaultAdjustmentAgreementService(IDefaultAdjustmentAgreementStore store, IClock clock)
    {
        return new DefaultAdjustmentAgreementService(
            store,
            DefaultAdjustmentAgreementRules.DefaultDefaultAdjustmentAgreementValidator(),
            clock,
            new List<string>());
    }
}
