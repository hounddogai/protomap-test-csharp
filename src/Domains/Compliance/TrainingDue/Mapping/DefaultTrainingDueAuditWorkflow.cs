namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default training due audits through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultTrainingDueAuditWorkflow(IDefaultTrainingDueAuditStore Store, DefaultTrainingDueAuditValidator Validator, IClock Clock, List<string> Journal, DefaultTrainingDueAuditEngine Engine)
{
    public static DefaultTrainingDueAuditWorkflow NewDefaultTrainingDueAuditWorkflow(IDefaultTrainingDueAuditStore store, IClock clock, Registry flags)
    {
        return new DefaultTrainingDueAuditWorkflow(
            store,
            DefaultTrainingDueAuditRules.DefaultDefaultTrainingDueAuditValidator(),
            clock,
            new List<string>(),
            DefaultTrainingDueAuditEngine.DefaultDefaultTrainingDueAuditEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default training due audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultTrainingDueAuditStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default training due audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultTrainingDueAuditStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default training due audit {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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
