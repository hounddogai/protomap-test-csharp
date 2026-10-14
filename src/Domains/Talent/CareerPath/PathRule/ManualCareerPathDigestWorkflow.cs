namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual career path digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualCareerPathDigestManager(IManualCareerPathDigestStore Store, ManualCareerPathDigestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual career path digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualCareerPathDigestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual career path digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualCareerPathDigestStatus.Expired);
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

/// <summary>Functions of manual career path digest workflow.</summary>
public static class ManualCareerPathDigestWorkflow
{
    public static ManualCareerPathDigestManager NewManualCareerPathDigestManager(IManualCareerPathDigestStore store, IClock clock)
    {
        return new ManualCareerPathDigestManager(
            store,
            ManualCareerPathDigestRules.DefaultManualCareerPathDigestValidator(),
            clock,
            new List<string>());
    }
}
