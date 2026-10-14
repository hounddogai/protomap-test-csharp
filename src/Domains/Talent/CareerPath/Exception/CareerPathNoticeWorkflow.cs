namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves career path notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathNoticeCoordinator(ICareerPathNoticeStore Store, CareerPathNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathNoticeStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathNoticeStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of career path notice workflow.</summary>
public static class CareerPathNoticeWorkflow
{
    public static CareerPathNoticeCoordinator NewCareerPathNoticeCoordinator(ICareerPathNoticeStore store, IClock clock)
    {
        return new CareerPathNoticeCoordinator(
            store,
            CareerPathNoticeRules.DefaultCareerPathNoticeValidator(),
            clock,
            new List<string>());
    }
}
