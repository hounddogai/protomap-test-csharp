namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pay run reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayRunReviewWorkflow(IPayRunReviewStore Store, PayRunReviewValidator Validator, IClock Clock, List<string> Journal, PayRunReviewEngine Engine)
{
    public static PayRunReviewWorkflow NewPayRunReviewWorkflow(IPayRunReviewStore store, IClock clock, Registry flags)
    {
        return new PayRunReviewWorkflow(
            store,
            PayRunReviewRules.DefaultPayRunReviewValidator(),
            clock,
            new List<string>(),
            PayRunReviewEngine.DefaultPayRunReviewEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayRunReviewStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayRunReviewStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run review {code} not found");
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
