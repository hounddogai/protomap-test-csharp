namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict career path digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictCareerPathDigestWorkflow(IStrictCareerPathDigestStore Store, StrictCareerPathDigestValidator Validator, IClock Clock, List<string> Journal, StrictCareerPathDigestEngine Engine)
{
    public static StrictCareerPathDigestWorkflow NewStrictCareerPathDigestWorkflow(IStrictCareerPathDigestStore store, IClock clock, Registry flags)
    {
        return new StrictCareerPathDigestWorkflow(
            store,
            StrictCareerPathDigestRules.DefaultStrictCareerPathDigestValidator(),
            clock,
            new List<string>(),
            StrictCareerPathDigestEngine.DefaultStrictCareerPathDigestEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict career path digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictCareerPathDigestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict career path digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictCareerPathDigestStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict career path digest {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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
