namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient desk templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientDeskTemplateWorkflow(ILenientDeskTemplateStore Store, LenientDeskTemplateValidator Validator, IClock Clock, List<string> Journal)
{
    public static LenientDeskTemplateWorkflow NewLenientDeskTemplateWorkflow(ILenientDeskTemplateStore store, IClock clock)
    {
        return new LenientDeskTemplateWorkflow(
            store,
            LenientDeskTemplateRules.DefaultLenientDeskTemplateValidator(),
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
            throw new InvalidOperationException($"lenient desk template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientDeskTemplateStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient desk template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientDeskTemplateStatus.Archived);
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
            if (item.StartsAt > cutoff)
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
