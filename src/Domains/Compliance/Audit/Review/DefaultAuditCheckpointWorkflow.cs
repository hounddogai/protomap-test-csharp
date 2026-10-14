namespace Acme.Workforce.Compliance.Audit.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default audit checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultAuditCheckpointService(IDefaultAuditCheckpointStore Store, DefaultAuditCheckpointValidator Validator, IClock Clock, List<string> Journal, DefaultAuditCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default audit checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultAuditCheckpointStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default audit checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultAuditCheckpointStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default audit checkpoint {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of default audit checkpoint workflow.</summary>
public static class DefaultAuditCheckpointWorkflow
{
    public static DefaultAuditCheckpointService NewDefaultAuditCheckpointService(IDefaultAuditCheckpointStore store, IClock clock, Registry flags)
    {
        return new DefaultAuditCheckpointService(
            store,
            DefaultAuditCheckpointRules.DefaultDefaultAuditCheckpointValidator(),
            clock,
            new List<string>(),
            DefaultAuditCheckpointEngine.DefaultDefaultAuditCheckpointEngine(flags));
    }
}
