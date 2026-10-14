namespace Acme.Workforce.Talent.CareerPath.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves career path manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathManifestWorkflow(ICareerPathManifestStore Store, CareerPathManifestValidator Validator, IClock Clock, List<string> Journal, CareerPathManifestEngine Engine)
{
    public static CareerPathManifestWorkflow NewCareerPathManifestWorkflow(ICareerPathManifestStore store, IClock clock)
    {
        return new CareerPathManifestWorkflow(
            store,
            CareerPathManifestRules.DefaultCareerPathManifestValidator(),
            clock,
            new List<string>(),
            CareerPathManifestEngine.DefaultCareerPathManifestEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathManifestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathManifestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path manifest {code} not found");
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
