namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual career path entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualCareerPathEntryDesk(IManualCareerPathEntryStore Store, ManualCareerPathEntryValidator Validator, IClock Clock, List<string> Journal, ManualCareerPathEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual career path entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualCareerPathEntryStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual career path entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualCareerPathEntryStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual career path entry {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of manual career path entry workflow.</summary>
public static class ManualCareerPathEntryWorkflow
{
    public static ManualCareerPathEntryDesk NewManualCareerPathEntryDesk(IManualCareerPathEntryStore store, IClock clock)
    {
        return new ManualCareerPathEntryDesk(
            store,
            ManualCareerPathEntryRules.DefaultManualCareerPathEntryValidator(),
            clock,
            new List<string>(),
            ManualCareerPathEntryEngine.DefaultManualCareerPathEntryEngine());
    }
}
