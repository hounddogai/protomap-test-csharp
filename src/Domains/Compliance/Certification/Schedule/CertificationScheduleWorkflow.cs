namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves certification schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationScheduleWorkflow(ICertificationScheduleStore Store, CertificationScheduleValidator Validator, IClock Clock, List<string> Journal, CertificationScheduleEngine Engine)
{
    public static CertificationScheduleWorkflow NewCertificationScheduleWorkflow(ICertificationScheduleStore store, IClock clock)
    {
        return new CertificationScheduleWorkflow(
            store,
            CertificationScheduleRules.DefaultCertificationScheduleValidator(),
            clock,
            new List<string>(),
            CertificationScheduleEngine.DefaultCertificationScheduleEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationScheduleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationScheduleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification schedule {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.UpdatedAt > cutoff)
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
