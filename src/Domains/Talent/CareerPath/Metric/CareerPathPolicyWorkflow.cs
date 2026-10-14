namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves career path policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathPolicyWorkflow(ICareerPathPolicyStore Store, CareerPathPolicyValidator Validator, IClock Clock, List<string> Journal, CareerPathPolicyEngine Engine)
{
    public static CareerPathPolicyWorkflow NewCareerPathPolicyWorkflow(ICareerPathPolicyStore store, IClock clock, Registry flags)
    {
        return new CareerPathPolicyWorkflow(
            store,
            CareerPathPolicyRules.DefaultCareerPathPolicyValidator(),
            clock,
            new List<string>(),
            CareerPathPolicyEngine.DefaultCareerPathPolicyEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathPolicyStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathPolicyStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path policy {code} not found");
        }
        var result = Engine.Compute(item);
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
