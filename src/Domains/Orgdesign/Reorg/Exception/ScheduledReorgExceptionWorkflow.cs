namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled reorg exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledReorgExceptionWorkflow(IScheduledReorgExceptionStore Store, ScheduledReorgExceptionValidator Validator, IClock Clock, List<string> Journal, ScheduledReorgExceptionEngine Engine)
{
    public static ScheduledReorgExceptionWorkflow NewScheduledReorgExceptionWorkflow(IScheduledReorgExceptionStore store, IClock clock, Registry flags)
    {
        return new ScheduledReorgExceptionWorkflow(
            store,
            ScheduledReorgExceptionRules.DefaultScheduledReorgExceptionValidator(),
            clock,
            new List<string>(),
            ScheduledReorgExceptionEngine.DefaultScheduledReorgExceptionEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled reorg exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledReorgExceptionStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled reorg exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledReorgExceptionStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled reorg exception {code} not found");
        }
        var result = Engine.Evaluate(item);
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
