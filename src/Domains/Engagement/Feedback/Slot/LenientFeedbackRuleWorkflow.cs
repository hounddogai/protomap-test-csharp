namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient feedback rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientFeedbackRuleDesk(ILenientFeedbackRuleStore Store, LenientFeedbackRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient feedback rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientFeedbackRuleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient feedback rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientFeedbackRuleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of lenient feedback rule workflow.</summary>
public static class LenientFeedbackRuleWorkflow
{
    public static LenientFeedbackRuleDesk NewLenientFeedbackRuleDesk(ILenientFeedbackRuleStore store, IClock clock)
    {
        return new LenientFeedbackRuleDesk(
            store,
            LenientFeedbackRuleRules.DefaultLenientFeedbackRuleValidator(),
            clock,
            new List<string>());
    }
}
