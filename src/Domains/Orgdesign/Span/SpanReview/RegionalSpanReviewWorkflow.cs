namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional span reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalSpanReviewWorkflow(IRegionalSpanReviewStore Store, RegionalSpanReviewValidator Validator, IClock Clock, List<string> Journal, RegionalSpanReviewEngine Engine)
{
    public static RegionalSpanReviewWorkflow NewRegionalSpanReviewWorkflow(IRegionalSpanReviewStore store, IClock clock)
    {
        return new RegionalSpanReviewWorkflow(
            store,
            RegionalSpanReviewRules.DefaultRegionalSpanReviewValidator(),
            clock,
            new List<string>(),
            RegionalSpanReviewEngine.DefaultRegionalSpanReviewEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional span review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalSpanReviewStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional span review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalSpanReviewStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional span review {code} not found");
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
