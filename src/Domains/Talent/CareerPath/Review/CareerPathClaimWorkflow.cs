namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves career path claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathClaimWorkflow(ICareerPathClaimStore Store, CareerPathClaimValidator Validator, IClock Clock, List<string> Journal, CareerPathClaimEngine Engine)
{
    public static CareerPathClaimWorkflow NewCareerPathClaimWorkflow(ICareerPathClaimStore store, IClock clock, Registry flags)
    {
        return new CareerPathClaimWorkflow(
            store,
            CareerPathClaimRules.DefaultCareerPathClaimValidator(),
            clock,
            new List<string>(),
            CareerPathClaimEngine.DefaultCareerPathClaimEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathClaimStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathClaimStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path claim {code} not found");
        }
        var result = Engine.Compute(item);
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
