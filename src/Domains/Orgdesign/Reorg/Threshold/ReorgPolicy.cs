namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg policy is in its life cycle.</summary>
public enum ReorgPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A reorg policy as the reorg team keeps it.</summary>
public sealed class ReorgPolicy(string code, string label, int limit, string grade, int startsAt, string externalRef, string team, ReorgPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public string Grade { get; set; } = grade;
    public int StartsAt { get; set; } = startsAt;
    public string ExternalRef { get; set; } = externalRef;
    public string Team { get; set; } = team;
    public ReorgPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a reorg policy in status new.</summary>
    public static ReorgPolicy NewReorgPolicy(string code, string label)
    {
        return new ReorgPolicy(code, label, 0, "", 0, "", "", ReorgPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgPolicyStatus next)
    {
        switch (Status)
        {
            case ReorgPolicyStatus.New:
                return next == ReorgPolicyStatus.Done || next == ReorgPolicyStatus.Failed;
            case ReorgPolicyStatus.Queued:
                return next == ReorgPolicyStatus.Running;
            case ReorgPolicyStatus.Running:
                switch (next)
                {
                    case ReorgPolicyStatus.Done:
                        return true;
                }
                return false;
            case ReorgPolicyStatus.Done:
                switch (next)
                {
                    case ReorgPolicyStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)Limit - (double)StartsAt, 2);
    }

    public ReorgPolicy Copy()
    {
        return new ReorgPolicy(Code, Label, Limit, Grade, StartsAt, ExternalRef, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
