namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves career path proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathProposalManager(ICareerPathProposalStore Store, CareerPathProposalValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathProposalStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathProposalStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of career path proposal workflow.</summary>
public static class CareerPathProposalWorkflow
{
    public static CareerPathProposalManager NewCareerPathProposalManager(ICareerPathProposalStore store, IClock clock)
    {
        return new CareerPathProposalManager(
            store,
            CareerPathProposalRules.DefaultCareerPathProposalValidator(),
            clock,
            new List<string>());
    }
}
