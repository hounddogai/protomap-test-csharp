namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves exception segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record ExceptionSegmentWorkflow(IExceptionSegmentStore Store, ExceptionSegmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static ExceptionSegmentWorkflow NewExceptionSegmentWorkflow(IExceptionSegmentStore store, IClock clock)
    {
        return new ExceptionSegmentWorkflow(
            store,
            ExceptionSegmentRules.DefaultExceptionSegmentValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ExceptionSegmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"exception segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ExceptionSegmentStatus.Expired);
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
