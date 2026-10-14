namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves feedback templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record FeedbackTemplateManager(IFeedbackTemplateStore Store, FeedbackTemplateValidator Validator, IClock Clock, List<string> Journal, FeedbackTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FeedbackTemplateStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FeedbackTemplateStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback template {code} not found");
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

/// <summary>Functions of feedback template workflow.</summary>
public static class FeedbackTemplateWorkflow
{
    public static FeedbackTemplateManager NewFeedbackTemplateManager(IFeedbackTemplateStore store, IClock clock, Registry flags)
    {
        return new FeedbackTemplateManager(
            store,
            FeedbackTemplateRules.DefaultFeedbackTemplateValidator(),
            clock,
            new List<string>(),
            FeedbackTemplateEngine.DefaultFeedbackTemplateEngine(flags));
    }
}
