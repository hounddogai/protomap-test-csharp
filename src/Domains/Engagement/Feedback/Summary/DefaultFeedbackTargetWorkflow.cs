namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default feedback targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultFeedbackTargetManager(IDefaultFeedbackTargetStore Store, DefaultFeedbackTargetValidatorExtra Validator, IClock Clock, List<string> Journal, DefaultFeedbackTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default feedback target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultFeedbackTargetStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default feedback target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultFeedbackTargetStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default feedback target {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of default feedback target workflow.</summary>
public static class DefaultFeedbackTargetWorkflow
{
    public static DefaultFeedbackTargetManager NewDefaultFeedbackTargetManager(IDefaultFeedbackTargetStore store, IClock clock)
    {
        return new DefaultFeedbackTargetManager(
            store,
            DefaultFeedbackTargetRules.DefaultDefaultFeedbackTargetValidator(),
            clock,
            new List<string>(),
            DefaultFeedbackTargetEngine.DefaultDefaultFeedbackTargetEngine());
    }
}
