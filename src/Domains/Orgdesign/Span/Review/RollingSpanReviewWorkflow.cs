namespace Acme.Workforce.Orgdesign.Span.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling span reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingSpanReviewManager(IRollingSpanReviewStore Store, RollingSpanReviewValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling span review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingSpanReviewStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling span review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingSpanReviewStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of rolling span review workflow.</summary>
public static class RollingSpanReviewWorkflow
{
    public static RollingSpanReviewManager NewRollingSpanReviewManager(IRollingSpanReviewStore store, IClock clock)
    {
        return new RollingSpanReviewManager(
            store,
            RollingSpanReviewRules.DefaultRollingSpanReviewValidator(),
            clock,
            new List<string>());
    }
}
