namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeTemplateDesk(IAccessBadgeTemplateStore Store, AccessBadgeTemplateValidator Validator, IClock Clock, List<string> Journal, AccessBadgeTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeTemplateStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeTemplateStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge template {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of access badge template workflow.</summary>
public static class AccessBadgeTemplateWorkflow
{
    public static AccessBadgeTemplateDesk NewAccessBadgeTemplateDesk(IAccessBadgeTemplateStore store, IClock clock)
    {
        return new AccessBadgeTemplateDesk(
            store,
            AccessBadgeTemplateRules.DefaultAccessBadgeTemplateValidator(),
            clock,
            new List<string>(),
            AccessBadgeTemplateEngine.DefaultAccessBadgeTemplateEngine());
    }
}
