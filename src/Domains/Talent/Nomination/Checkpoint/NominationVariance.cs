namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination variance is in its life cycle.</summary>
public enum NominationVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A nomination variance. Its code is unique within a tenant.</summary>
public sealed class NominationVariance(string code, string label, int expiresAt, string grade, int capacity, string contactEmail, int leaveDays, NominationVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Grade { get; set; } = grade;
    public int Capacity { get; set; } = capacity;
    public string ContactEmail { get; set; } = contactEmail;
    public int LeaveDays { get; set; } = leaveDays;
    public NominationVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a nomination variance in status draft.</summary>
    public static NominationVariance NewNominationVariance(string code, string label)
    {
        return new NominationVariance(code, label, 0, "", 0, "", 0, NominationVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationVarianceStatus next)
    {
        switch (Status)
        {
            case NominationVarianceStatus.Draft:
                return next == NominationVarianceStatus.Pending;
            case NominationVarianceStatus.Pending:
                switch (next)
                {
                    case NominationVarianceStatus.Active:
                    case NominationVarianceStatus.Closed:
                        return true;
                }
                return false;
            case NominationVarianceStatus.Active:
                return next == NominationVarianceStatus.Closed;
            case NominationVarianceStatus.Suspended:
                return next == NominationVarianceStatus.Closed;
        }
        return false;
    }

    public void MoveTo(NominationVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)ExpiresAt - (double)Capacity, 2);
    }

    public NominationVariance Copy()
    {
        return new NominationVariance(Code, Label, ExpiresAt, Grade, Capacity, ContactEmail, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
