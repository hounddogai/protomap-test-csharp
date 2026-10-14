namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped access badge templates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedAccessBadgeTemplateService(ICappedAccessBadgeTemplateStore Store, CappedAccessBadgeTemplateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped access badge template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedAccessBadgeTemplateStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped access badge template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedAccessBadgeTemplateStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of capped access badge template workflow.</summary>
public static class CappedAccessBadgeTemplateWorkflow
{
    public static CappedAccessBadgeTemplateService NewCappedAccessBadgeTemplateService(ICappedAccessBadgeTemplateStore store, IClock clock)
    {
        return new CappedAccessBadgeTemplateService(
            store,
            CappedAccessBadgeTemplateRules.DefaultCappedAccessBadgeTemplateValidator(),
            clock,
            new List<string>());
    }
}
