namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient access badge estimates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientAccessBadgeEstimateWorkflow(ILenientAccessBadgeEstimateStore Store, LenientAccessBadgeEstimateValidator Validator, IClock Clock, List<string> Journal, LenientAccessBadgeEstimateEngine Engine)
{
    public static LenientAccessBadgeEstimateWorkflow NewLenientAccessBadgeEstimateWorkflow(ILenientAccessBadgeEstimateStore store, IClock clock, Registry flags)
    {
        return new LenientAccessBadgeEstimateWorkflow(
            store,
            LenientAccessBadgeEstimateRules.DefaultLenientAccessBadgeEstimateValidator(),
            clock,
            new List<string>(),
            LenientAccessBadgeEstimateEngine.DefaultLenientAccessBadgeEstimateEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient access badge estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientAccessBadgeEstimateStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient access badge estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientAccessBadgeEstimateStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient access badge estimate {code} not found");
        }
        var result = Engine.Evaluate(item);
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
