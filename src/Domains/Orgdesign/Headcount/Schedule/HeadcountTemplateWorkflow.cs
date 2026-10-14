namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves headcount templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountTemplateWorkflow(IHeadcountTemplateStore Store, HeadcountTemplateValidator Validator, IClock Clock, List<string> Journal)
{
    public static HeadcountTemplateWorkflow NewHeadcountTemplateWorkflow(IHeadcountTemplateStore store, IClock clock)
    {
        return new HeadcountTemplateWorkflow(
            store,
            HeadcountTemplateRules.DefaultHeadcountTemplateValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountTemplateStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountTemplateStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
