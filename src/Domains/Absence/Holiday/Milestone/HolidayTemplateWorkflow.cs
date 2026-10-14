namespace Acme.Workforce.Absence.Holiday.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves holiday templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayTemplateCoordinator(IHolidayTemplateStore Store, HolidayTemplateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayTemplateStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayTemplateStatus.Archived);
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

/// <summary>Functions of holiday template workflow.</summary>
public static class HolidayTemplateWorkflow
{
    public static HolidayTemplateCoordinator NewHolidayTemplateCoordinator(IHolidayTemplateStore store, IClock clock)
    {
        return new HolidayTemplateCoordinator(
            store,
            HolidayTemplateRules.DefaultHolidayTemplateValidator(),
            clock,
            new List<string>());
    }
}
