namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves visitor reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorReviewWorkflow(IVisitorReviewStore Store, VisitorReviewValidator Validator, IClock Clock, List<string> Journal, VisitorReviewEngine Engine)
{
    public static VisitorReviewWorkflow NewVisitorReviewWorkflow(IVisitorReviewStore store, IClock clock, Registry flags)
    {
        return new VisitorReviewWorkflow(
            store,
            VisitorReviewRules.DefaultVisitorReviewValidator(),
            clock,
            new List<string>(),
            VisitorReviewEngine.DefaultVisitorReviewEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorReviewStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorReviewStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor review {code} not found");
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
            if (item.ExpiresAt > cutoff)
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
