namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary feedback notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryFeedbackNoticeWorkflow(IPrimaryFeedbackNoticeStore Store, PrimaryFeedbackNoticeValidator Validator, IClock Clock, List<string> Journal, PrimaryFeedbackNoticeEngine Engine)
{
    public static PrimaryFeedbackNoticeWorkflow NewPrimaryFeedbackNoticeWorkflow(IPrimaryFeedbackNoticeStore store, IClock clock, Registry flags)
    {
        return new PrimaryFeedbackNoticeWorkflow(
            store,
            PrimaryFeedbackNoticeRules.DefaultPrimaryFeedbackNoticeValidator(),
            clock,
            new List<string>(),
            PrimaryFeedbackNoticeEngine.DefaultPrimaryFeedbackNoticeEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary feedback notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryFeedbackNoticeStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary feedback notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryFeedbackNoticeStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary feedback notice {code} not found");
        }
        var result = Engine.Evaluate(item);
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
