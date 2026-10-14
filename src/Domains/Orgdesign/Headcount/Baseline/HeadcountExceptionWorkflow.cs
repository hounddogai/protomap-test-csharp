namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves headcount exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountExceptionWorkflow(IHeadcountExceptionStore Store, HeadcountExceptionValidator Validator, IClock Clock, List<string> Journal, HeadcountExceptionEngine Engine)
{
    public static HeadcountExceptionWorkflow NewHeadcountExceptionWorkflow(IHeadcountExceptionStore store, IClock clock, Registry flags)
    {
        return new HeadcountExceptionWorkflow(
            store,
            HeadcountExceptionRules.DefaultHeadcountExceptionValidator(),
            clock,
            new List<string>(),
            HeadcountExceptionEngine.DefaultHeadcountExceptionEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountExceptionStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountExceptionStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount exception {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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
