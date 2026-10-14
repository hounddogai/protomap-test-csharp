namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary engagement notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryEngagementNoticeWorkflow(IPrimaryEngagementNoticeStore Store, PrimaryEngagementNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    public static PrimaryEngagementNoticeWorkflow NewPrimaryEngagementNoticeWorkflow(IPrimaryEngagementNoticeStore store, IClock clock)
    {
        return new PrimaryEngagementNoticeWorkflow(
            store,
            PrimaryEngagementNoticeRules.DefaultPrimaryEngagementNoticeValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary engagement notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryEngagementNoticeStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary engagement notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryEngagementNoticeStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
            if (item.EndsAt > cutoff)
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
