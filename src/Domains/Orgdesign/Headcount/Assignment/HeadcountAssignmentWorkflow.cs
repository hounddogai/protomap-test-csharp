namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves headcount assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountAssignmentService(IHeadcountAssignmentStore Store, HeadcountAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountAssignmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountAssignmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of headcount assignment workflow.</summary>
public static class HeadcountAssignmentWorkflow
{
    public static HeadcountAssignmentService NewHeadcountAssignmentService(IHeadcountAssignmentStore store, IClock clock)
    {
        return new HeadcountAssignmentService(
            store,
            HeadcountAssignmentRules.DefaultHeadcountAssignmentValidator(),
            clock,
            new List<string>());
    }
}
