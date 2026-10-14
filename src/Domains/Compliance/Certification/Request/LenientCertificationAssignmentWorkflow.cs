namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient certification assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientCertificationAssignmentWorkflow(ILenientCertificationAssignmentStore Store, LenientCertificationAssignmentValidator Validator, IClock Clock, List<string> Journal, LenientCertificationAssignmentEngine Engine)
{
    public static LenientCertificationAssignmentWorkflow NewLenientCertificationAssignmentWorkflow(ILenientCertificationAssignmentStore store, IClock clock)
    {
        return new LenientCertificationAssignmentWorkflow(
            store,
            LenientCertificationAssignmentRules.DefaultLenientCertificationAssignmentValidator(),
            clock,
            new List<string>(),
            LenientCertificationAssignmentEngine.DefaultLenientCertificationAssignmentEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient certification assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientCertificationAssignmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient certification assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientCertificationAssignmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient certification assignment {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.CreatedAt > cutoff)
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
