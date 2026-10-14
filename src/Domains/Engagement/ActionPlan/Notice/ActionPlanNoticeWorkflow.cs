namespace Acme.Workforce.Engagement.ActionPlan.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanNoticeService(IActionPlanNoticeStore Store, ActionPlanNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanNoticeStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanNoticeStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of action plan notice workflow.</summary>
public static class ActionPlanNoticeWorkflow
{
    public static ActionPlanNoticeService NewActionPlanNoticeService(IActionPlanNoticeStore store, IClock clock)
    {
        return new ActionPlanNoticeService(
            store,
            ActionPlanNoticeRules.DefaultActionPlanNoticeValidator(),
            clock,
            new List<string>());
    }
}
