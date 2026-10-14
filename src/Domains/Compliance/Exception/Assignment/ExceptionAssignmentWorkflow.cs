namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves exception assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionAssignmentManager(IExceptionAssignmentStore Store, ExceptionAssignmentValidator Validator, IClock Clock, List<string> Journal, ExceptionAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionAssignmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionAssignmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception assignment {code} not found");
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

/// <summary>Functions of exception assignment workflow.</summary>
public static class ExceptionAssignmentWorkflow
{
    public static ExceptionAssignmentManager NewExceptionAssignmentManager(IExceptionAssignmentStore store, IClock clock)
    {
        return new ExceptionAssignmentManager(
            store,
            ExceptionAssignmentRules.DefaultExceptionAssignmentValidator(),
            clock,
            new List<string>(),
            ExceptionAssignmentEngine.DefaultExceptionAssignmentEngine());
    }
}
