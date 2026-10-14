namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves headcount batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountBatchWorkflow(IHeadcountBatchStore Store, HeadcountBatchValidator Validator, IClock Clock, List<string> Journal, HeadcountBatchEngine Engine)
{
    public static HeadcountBatchWorkflow NewHeadcountBatchWorkflow(IHeadcountBatchStore store, IClock clock, Registry flags)
    {
        return new HeadcountBatchWorkflow(
            store,
            HeadcountBatchRules.DefaultHeadcountBatchValidator(),
            clock,
            new List<string>(),
            HeadcountBatchEngine.DefaultHeadcountBatchEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountBatchStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountBatchStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount batch {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.CreatedAt > cutoff)
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
