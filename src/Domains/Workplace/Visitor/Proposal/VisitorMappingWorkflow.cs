namespace Acme.Workforce.Workplace.Visitor.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorMappingService(IVisitorMappingStore Store, VisitorMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorMappingStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorMappingStatus.Archived);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of visitor mapping workflow.</summary>
public static class VisitorMappingWorkflow
{
    public static VisitorMappingService NewVisitorMappingService(IVisitorMappingStore store, IClock clock)
    {
        return new VisitorMappingService(
            store,
            VisitorMappingRules.DefaultVisitorMappingValidator(),
            clock,
            new List<string>());
    }
}
