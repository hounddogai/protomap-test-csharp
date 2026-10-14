namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation plan is in its life cycle.</summary>
public enum AttestationPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A attestation plan. Its code is unique within a tenant.</summary>
public sealed class AttestationPlan(string code, string title, int batchSize, int limit, string currency, string summary, int graceDays, string name, int leaveDays, string team, string notes, AttestationPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public int Limit { get; set; } = limit;
    public string Currency { get; set; } = currency;
    public string Summary { get; set; } = summary;
    public int GraceDays { get; set; } = graceDays;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public string Team { get; set; } = team;
    public string Notes { get; set; } = notes;
    public AttestationPlanStatus Status { get; set; } = status;

    /// <summary>Starts a attestation plan in status draft.</summary>
    public static AttestationPlan NewAttestationPlan(string code, string title)
    {
        return new AttestationPlan(code, title, 0, 0, "", "", 0, "", 0, "", "", AttestationPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationPlanStatus next)
    {
        switch (Status)
        {
            case AttestationPlanStatus.Draft:
                return next == AttestationPlanStatus.Active || next == AttestationPlanStatus.Closed;
            case AttestationPlanStatus.Pending:
                switch (next)
                {
                    case AttestationPlanStatus.Active:
                        return true;
                }
                return false;
            case AttestationPlanStatus.Active:
                return next == AttestationPlanStatus.Suspended || next == AttestationPlanStatus.Closed;
            case AttestationPlanStatus.Suspended:
                switch (next)
                {
                    case AttestationPlanStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"attestation plan"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Limit * (double)BatchSize, 2);
    }

    public AttestationPlan Copy()
    {
        return new AttestationPlan(
            Code,
            Title,
            BatchSize,
            Limit,
            Currency,
            Summary,
            GraceDays,
            Name,
            LeaveDays,
            Team,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
