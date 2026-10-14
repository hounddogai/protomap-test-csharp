namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves career path assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathAssignmentWorkflow(ICareerPathAssignmentStore Store, CareerPathAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static CareerPathAssignmentWorkflow NewCareerPathAssignmentWorkflow(ICareerPathAssignmentStore store, IClock clock)
    {
        return new CareerPathAssignmentWorkflow(
            store,
            CareerPathAssignmentRules.DefaultCareerPathAssignmentValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathAssignmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathAssignmentStatus.Stopped);
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
