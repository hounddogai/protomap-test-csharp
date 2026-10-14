namespace Acme.Workforce.Compliance.Attestation.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation score is in its life cycle.</summary>
public enum AttestationScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One attestation score, with what the planners need to act on it.</summary>
public sealed class AttestationScore(string code, string title, string region, string location, double cost, int batchSize, string department, AttestationScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string Location { get; set; } = location;
    public double Cost { get; set; } = cost;
    public int BatchSize { get; set; } = batchSize;
    public string Department { get; set; } = department;
    public AttestationScoreStatus Status { get; set; } = status;

    /// <summary>Starts a attestation score in status draft.</summary>
    public static AttestationScore NewAttestationScore(string code, string title)
    {
        return new AttestationScore(code, title, "", "", 0.0, 0, "", AttestationScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationScoreStatus next)
    {
        switch (Status)
        {
            case AttestationScoreStatus.Draft:
                switch (next)
                {
                    case AttestationScoreStatus.Pending:
                        return true;
                }
                return false;
            case AttestationScoreStatus.Pending:
                return next == AttestationScoreStatus.Active || next == AttestationScoreStatus.Suspended;
            case AttestationScoreStatus.Active:
                switch (next)
                {
                    case AttestationScoreStatus.Suspended:
                        return true;
                }
                return false;
            case AttestationScoreStatus.Suspended:
                switch (next)
                {
                    case AttestationScoreStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)BatchSize - Cost, 2);
    }

    public AttestationScore Copy()
    {
        return new AttestationScore(Code, Title, Region, Location, Cost, BatchSize, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
